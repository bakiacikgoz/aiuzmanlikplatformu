import { expect, test } from '@playwright/test'
import { apiBase, loginAdmin, registerLearner, token } from './helpers'

test('admin can create edit preview and archive a micro lesson', async ({ page, request }) => {
  await loginAdmin(page)
  const adminToken = await token(page)
  const slug = `e2e-ai-byte-${Date.now()}`
  const title = `E2E AI Byte ${Date.now()}`

  await page.goto('/admin/content/lessons/new')
  await page.getByLabel('Unit slug').fill('ai-okuryazarligi')
  await page.getByLabel('Ders slug').fill(slug)
  await page.getByLabel('Baslik').fill(title)
  await page.getByLabel('Sure dakika').fill('4')
  await page.getByLabel('XP').fill('10')
  await page.getByLabel('Sira').fill('20')
  await page.getByRole('button', { name: 'Icerik' }).click()
  await page.getByLabel('Ogrenme hedefi').fill('Kullanici tek bir AI kavramini kendi ornegiyle aciklar.')
  await page.getByLabel('Mini aciklama').fill('Bu mikro ders, bir AI kavramini is akisi icindeki kucuk bir karar noktasi olarak dusunmeyi saglar.')
  await page.getByLabel('Mini ornek').fill('Ornek: Destek mesajini once niyete gore siniflandir, sonra insan kontrolu gerekenleri ayir.')
  await page.getByLabel('Sonraki adim').fill('Bir sonraki derste ayni kavram icin karsi ornek yaz.')
  await page.getByRole('button', { name: 'Egzersiz' }).click()
  await page.getByLabel('Egzersiz promptu').fill('Kendi is akisin icin bir AI karar noktasi yaz.')
  await page.getByRole('button', { name: 'Kaynaklar' }).click()
  await page.locator('input[type="checkbox"]').first().check()
  await page.getByRole('button', { name: /Draft kaydet/i }).click()
  await expect(page).toHaveURL(/\/admin\/content\/lessons\/.+\/edit/)

  await page.getByRole('button', { name: /Review yap/i }).click()
  await page.getByRole('button', { name: /Publish/i }).click()
  await expect(page.getByText(/Degisiklikler kaydedildi/i)).toBeVisible()

  const match = page.url().match(/lessons\/([^/]+)\/edit/)
  expect(match?.[1]).toBeTruthy()
  const lessonId = match![1]

  await page.goto(`/admin/content/lessons/${lessonId}/preview`)
  await expect(page.getByText(title)).toBeVisible()
  await expect(page.getByText(/1\/7/)).toBeVisible()

  await page.evaluate(() => localStorage.clear())
  await registerLearner(page)
  await page.goto('/paths/beginner')
  await expect(page.getByText(title)).toBeVisible()

  await request.patch(`${apiBase}/admin/lessons/${lessonId}/status`, {
    headers: { Authorization: `Bearer ${adminToken}` },
    data: { status: 'Archived' },
  })
  await page.reload()
  await expect(page.getByText(title)).toHaveCount(0)
})
