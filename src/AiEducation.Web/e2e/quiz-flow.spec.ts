import { expect, test } from '@playwright/test'
import { apiBase, loginAdmin, registerLearner, token } from './helpers'

test('admin can create quiz questions and student score is calculated by backend', async ({ page, request }) => {
  await loginAdmin(page)
  const adminToken = await token(page)
  const lessonsResponse = await request.get(`${apiBase}/admin/lessons`, {
    headers: { Authorization: `Bearer ${adminToken}` },
  })
  const lessons = await lessonsResponse.json()
  const checkpoint = lessons.find((lesson: { slug: string }) => lesson.slug === 'beginner-ai-okuryazarligi-checkpoint')
  expect(checkpoint).toBeTruthy()

  await page.goto('/admin/content/quizzes')
  await page.locator('select').selectOption(checkpoint.id)

  for (let index = 1; index <= 3; index += 1) {
    await page.getByLabel('Soru metni').fill(`E2E quiz sorusu ${index}`)
    await page.getByLabel('Aciklama').fill(`E2E aciklama ${index}`)
    await page.getByLabel('Sira').fill(`${20 + index}`)
    await page.getByLabel('Secenek 1').fill('Dogru uygulama')
    await page.getByLabel('Secenek 2').fill('Yanlis uygulama')
    await page.getByLabel('Secenek 3').fill('Eksik uygulama')
    await page.getByLabel('Secenek 4').fill('Ilgisiz uygulama')
    await page.locator('input[name="correct-option"]').first().check()
    await page.getByRole('button', { name: /Soru ekle/i }).click()
    await expect(page.getByText(/Soru kaydedildi/i)).toBeVisible()
  }

  await page.evaluate(() => localStorage.clear())
  await registerLearner(page)
  await page.goto('/quiz')
  await expect(page.locator('fieldset').first()).toBeVisible()
  const questionCount = await page.locator('fieldset').count()
  for (let index = 0; index < questionCount; index += 1) {
    await page.locator('fieldset').nth(index).locator('input[type="radio"]').first().check()
  }

  await page.getByRole('button', { name: /Quiz/i }).click()
  await expect(page.getByText(/Skor 100/i)).toBeVisible()
  await expect(page.getByText(/XP/i)).toBeVisible()
})
