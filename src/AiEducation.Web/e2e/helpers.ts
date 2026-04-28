import { expect, type Page } from '@playwright/test'

export const apiBase = 'http://localhost:5076/api/v1'

export function uniqueEmail(prefix: string) {
  return `${prefix}-${Date.now()}-${Math.round(Math.random() * 1_000_000)}@example.com`
}

export async function registerLearner(page: Page, email = uniqueEmail('learner')) {
  await page.goto('/login')
  await page.locator('input').nth(0).fill('E2E Learner')
  await page.locator('input').nth(1).fill(email)
  await page.locator('input').nth(2).fill('Passw0rd!')
  await page.locator('button[type="submit"]').click()
  await page.waitForURL(/onboarding/)
  await page.getByRole('button', { name: /20 XP/i }).click()
  await page.locator('section').nth(1).locator('button').first().click()
  await page.locator('button').last().click()
  await expect(page).toHaveURL(/\/$/)
}

export async function loginAdmin(page: Page) {
  await page.goto('/login')
  await page.locator('button').last().click()
  await page.locator('input').nth(0).fill('admin@example.com')
  await page.locator('input').nth(1).fill('Admin123!')
  await page.locator('button[type="submit"]').click()
  await page.waitForURL(/onboarding|\/$/)
}

export async function token(page: Page) {
  const value = await page.evaluate(() => localStorage.getItem('ai_education_token'))
  if (!value) throw new Error('Missing auth token')
  return value
}
