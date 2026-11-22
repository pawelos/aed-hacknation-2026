import { expect, test } from '@playwright/test'
import { signIn, signInToPanel } from './support/accounts'

test.describe('Sign-in to the holder panel', { tag: '@smoke' }, () => {
    test('the panel without a session leads to the sign-in page', async ({ page }) => {
        await page.goto('/pl/panel/aeds')
        await expect(page).toHaveURL(/\/pl\/sign-in/)
        await expect(page.getByRole('heading', { level: 1, name: 'Panel dysponenta' })).toBeVisible()
    })

    test('a wrong password shows the error under the field and keeps the e-mail', async ({ page }) => {
        await signIn(page, 'holder', 'wrong-password-123')
        const password = page.getByLabel(/^Hasło/)
        await expect(password).toHaveAttribute('aria-invalid', 'true')
        await expect(password).toHaveAccessibleDescription('Nieprawidłowy e-mail lub hasło.')
        await expect(page.getByLabel(/^E-mail/)).toHaveValue('wroclaw@aed.local')
        await expect(page).toHaveURL(/\/pl\/sign-in/)
    })

    test('the holder signs in, sees the dashboard and signs out', async ({ page }) => {
        await signInToPanel(page, 'holder')
        await expect(page.getByRole('heading', { level: 1, name: 'Pulpit' })).toBeVisible()
        await expect(page.getByText('Rekordy AED').first()).toBeVisible()

        await page.getByRole('button', { name: 'Wyloguj' }).click()
        await page.waitForURL(/\/pl(\/sign-in)?\/?$/, { timeout: 10_000 })
        await page.goto('/pl/panel')
        await expect(page).toHaveURL(/\/pl\/sign-in/)
    })
})
