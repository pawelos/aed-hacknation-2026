import { defineConfig, devices } from '@playwright/test'
import dotenv from 'dotenv'

dotenv.config({ quiet: true })

/**
 * E2E tests against an already running prototype (Aspire, or the API and `npm run dev` / `npm start` in web/)
 * with the seeded demo data. The web address comes from E2E_BASE_URL (default: the dev server on 3060).
 */
export default defineConfig({
    testDir: './tests',
    fullyParallel: false,
    // Tests share one database (and the report rate limit of one address): run them one at a time.
    workers: 1,
    retries: process.env.CI ? 1 : 0,
    timeout: 60_000,
    expect: { timeout: 10_000 },
    reporter: [['list'], ['html', { open: 'never' }]],
    use: {
        baseURL: process.env.E2E_BASE_URL || 'http://localhost:3060',
        locale: 'pl-PL',
        timezoneId: 'Europe/Warsaw',
        trace: 'retain-on-failure',
        screenshot: 'only-on-failure',
    },
    projects: [
        { name: 'setup', testMatch: /auth\.setup\.ts/ },
        {
            name: 'chromium',
            use: { ...devices['Desktop Chrome'] },
            dependencies: ['setup'],
        },
    ],
})
