import { defineConfig, devices } from '@playwright/test'
import { fileURLToPath } from 'node:url'

const e2eDbPath = fileURLToPath(new URL('./ai_education_platform_v2_e2e.db', import.meta.url))

export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,
  expect: { timeout: 12_000 },
  fullyParallel: false,
  reporter: [['list']],
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: [
    {
      command: 'powershell -NoProfile -Command "Remove-Item -Force ai_education_platform_v2_e2e.db* -ErrorAction SilentlyContinue; dotnet run --project ../AiEducation.Api/AiEducation.Api.csproj --launch-profile http"',
      url: 'http://localhost:5076/api/v1/learning-paths',
      timeout: 120_000,
      reuseExistingServer: false,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        DatabaseProvider: 'Sqlite',
        ConnectionStrings__SqliteConnection: `Data Source=${e2eDbPath}`,
        Jwt__SigningKey: 'playwright-signing-key-change-before-production-32chars',
      },
    },
    {
      command: 'npm run dev -- --host 127.0.0.1',
      url: 'http://localhost:5173',
      timeout: 120_000,
      reuseExistingServer: false,
    },
  ],
})
