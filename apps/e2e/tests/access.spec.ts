import { expect, test } from '@playwright/test'
import { storageStateOf } from './support/accounts'

const WROCLAW_SOURCE = 'Rejestr AED w budynkach miejskich Wrocławia (dane przykładowe)'
const OSP_SOURCE = 'AED w remizach OSP (dane przykładowe)'

/** A holder sees only the data of their organisation; the administrator sees every holder. */
test.describe('Access to the data of holders', { tag: '@smoke' }, () => {
    test.describe('holder', () => {
        test.use({ storageState: storageStateOf('holder') })

        test('the dashboard lists only the own data source', async ({ page }) => {
            await page.goto('/pl/panel')
            await expect(page.getByRole('heading', { level: 1, name: 'Pulpit' })).toBeVisible()
            await expect(page.getByText('Urząd Miejski Wrocławia (demo)').first()).toBeVisible()
            await expect(page.getByText(WROCLAW_SOURCE).first()).toBeVisible()
            await expect(page.getByText(OSP_SOURCE)).toHaveCount(0)
            await expect(page.getByText('Administrator (wszyscy dysponenci)')).toHaveCount(0)
        })
    })

    test.describe('administrator', () => {
        test.use({ storageState: storageStateOf('admin') })

        test('the dashboard lists the data sources of every holder', async ({ page }) => {
            await page.goto('/pl/panel')
            await expect(page.getByText('Administrator (wszyscy dysponenci)').first()).toBeVisible()
            await expect(page.getByText(WROCLAW_SOURCE).first()).toBeVisible()
            await expect(page.getByText(OSP_SOURCE).first()).toBeVisible()
        })
    })
})
