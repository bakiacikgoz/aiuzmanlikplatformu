import { expect, test } from '@playwright/test'
import { registerLearner } from './helpers'

test('student can complete first AI Byte and see XP, streak, quest progress', async ({ page }) => {
  await registerLearner(page)

  await expect(page.getByText(/Toplam XP/i)).toBeVisible()
  const firstLessonLink = page.locator('a[href^="/lessons/"]').first()
  await firstLessonLink.click()

  await expect(page.getByText(/1\/7/)).toBeVisible()
  await page.getByRole('button', { name: /Sonraki/i }).click()
  await page.getByRole('button', { name: /Sonraki/i }).click()
  await page.getByRole('button', { name: /Sonraki/i }).click()
  await page.getByRole('button', { name: /Sonraki/i }).click()
  await page.getByRole('button', { name: /Sonraki/i }).click()
  await page.getByRole('button', { name: /Sonraki/i }).click()
  await expect(page.locator('button:disabled').filter({ hasText: /Al|Dersi/i })).toBeVisible()
  await page.getByRole('button', { name: /Geri/i }).click()
  await page.getByRole('button', { name: /Geri/i }).click()
  await page.getByRole('button', { name: /Geri/i }).click()

  await page.getByLabel('Cevap').fill('Bu kavrami kendi is akisimda tek bir karar noktasina baglayarak uygularim.')
  await page.locator('form').filter({ has: page.getByLabel('Cevap') }).locator('button[type="submit"]').click()
  await expect(page.getByText(/feedback|kaydedildi|Cevab/i)).toBeVisible()

  await page.getByRole('button', { name: /Sonraki/i }).click()
  await page.getByRole('button', { name: /Sonraki/i }).click()
  await page.getByRole('button', { name: /Sonraki/i }).click()
  await page.getByRole('button', { name: /Dersi tamamla/i }).click()
  await expect(page.getByRole('button', { name: /Tamamland/i })).toBeVisible()

  await page.goto('/')
  await expect(page.getByText(/Toplam XP/i)).toBeVisible()
  await expect(page.locator('p.text-2xl').filter({ hasText: /XP/ })).toBeVisible()
  await expect(page.getByText('Seri', { exact: true })).toBeVisible()

  await page.goto('/quests')
  await expect(page.getByText(/Tamamland|Bekliyor/i).first()).toBeVisible()

  await page.goto('/leagues')
  await expect(page.getByText(/E2E Learner/i)).toBeVisible()
})
