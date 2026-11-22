import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { expect, type Page } from '@playwright/test'

/** `holder`: the Wrocław city office (one data source); `admin`: every holder. */
export type Role = 'admin' | 'holder'

/** Seeded demo accounts (api/Aed/Api/appsettings.Development.json); overridable via env. */
export const ACCOUNTS: Record<Role, { email: string; password: string }> = {
    admin: {
        email: process.env.E2E_ADMIN_EMAIL || 'admin@aed.local',
        password: process.env.E2E_ADMIN_PASSWORD || 'AedAdmin2026',
    },
    holder: {
        email: process.env.E2E_HOLDER_EMAIL || 'wroclaw@aed.local',
        password: process.env.E2E_HOLDER_PASSWORD || 'AedDemo2026x',
    },
}

const authDir = path.join(path.dirname(fileURLToPath(import.meta.url)), '..', '..', '.auth')

/** Saved signed-in session of a role (written by auth.setup.ts). */
export const storageStateOf = (role: Role) => path.join(authDir, `${role}.json`)

/**
 * Fills and sends the form of /pl/sign-in. Values typed before React hydrates are lost (the inputs are
 * controlled and start with the first demo account), so the fields are filled again until they keep the values.
 */
export async function signIn(page: Page, role: Role, password = ACCOUNTS[role].password) {
    const { email } = ACCOUNTS[role]
    await page.goto('/pl/sign-in')
    await page.waitForLoadState('networkidle')
    const emailInput = page.getByLabel(/^E-mail/)
    const passwordInput = page.getByLabel(/^Hasło/)
    await expect(async () => {
        await emailInput.fill(email)
        await passwordInput.fill(password)
        await expect(emailInput).toHaveValue(email, { timeout: 1_000 })
        await expect(passwordInput).toHaveValue(password, { timeout: 1_000 })
    }).toPass({ timeout: 15_000 })
    await page.getByRole('button', { name: 'Zaloguj', exact: true }).click()
}

/** Signs in and waits for the holder panel. */
export async function signInToPanel(page: Page, role: Role) {
    await signIn(page, role)
    await page.waitForURL(/\/pl\/panel/, { timeout: 20_000 })
}
