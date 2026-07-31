import { defineConfig, devices } from '@playwright/test'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const startLocal = process.env.E2E_START_LOCAL !== '0'
const port = process.env.E2E_SERVER_PORT ?? '5051'
const baseURL = process.env.SITE_E2E_BASE_URL ?? (startLocal ? `http://127.0.0.1:${port}` : 'https://andymeier.dev')

export default defineConfig({
  testDir: './tests',
  timeout: 30_000,
  expect: { timeout: 10_000 },
  fullyParallel: false,
  workers: 1,
  retries: process.env.CI ? 2 : 0,
  reporter: process.env.CI ? [['list'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL,
    trace: 'retain-on-failure',
    video: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [{ name: 'firefox', use: { ...devices['Desktop Firefox'] } }],
  webServer: startLocal
    ? {
        command: 'bash scripts/start-local.sh',
        cwd: __dirname,
        url: `${baseURL}/health`,
        timeout: process.env.CI ? 300_000 : 120_000,
        reuseExistingServer: process.env.E2E_REUSE_EXISTING_SERVER === '1',
      }
    : undefined,
})
