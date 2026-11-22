import { expect, test, type APIRequestContext, type Page } from '@playwright/test'
import { storageStateOf } from './support/accounts'
import { apiAs, createAed, removeE2eAeds, uniqueName, valueOf } from './support/api'

/**
 * AED records of the holder panel: search, the edit drawer, field validation and the bottom selection toolbar.
 * The record under test is created through the API (the form with five lookups is not what these tests check).
 */
test.describe('Panel: AED records', { tag: '@critical' }, () => {
    test.use({ storageState: storageStateOf('holder') })

    let api: APIRequestContext

    test.beforeAll(async () => {
        api = await apiAs('holder')
        await removeE2eAeds(api)
    })

    test.afterAll(async () => {
        await removeE2eAeds(api)
        await api.dispose()
    })

    /** Opens the list filtered to the name; waits for the row. */
    async function findInList(page: Page, name: string) {
        await page.goto('/pl/panel/aeds')
        await page.waitForLoadState('networkidle')
        const search = page.getByRole('searchbox', { name: 'Szukaj rekordów' })
        await expect(async () => {
            await search.fill(name)
            await search.press('Enter')
            await expect(page).toHaveURL(new RegExp(`[?&]q=${name}`), { timeout: 2_000 })
        }).toPass({ timeout: 15_000 })
        const row = page.getByRole('row').filter({ hasText: name })
        await expect(row).toHaveCount(1)
        return row
    }

    test('search finds the record; the drawer saves a change', async ({ page }) => {
        const name = uniqueName('aed')
        const id = await createAed(api, name)

        const row = await findInList(page, name)
        await row.getByRole('button', { name: `Otwórz rekord ${name}` }).or(row.getByRole('link', { name })).first().click()

        const drawer = page.getByRole('dialog', { name: new RegExp(name) })
        await expect(drawer).toBeVisible()
        await expect(page).toHaveURL(new RegExp(`id=${id}`))

        const placement = drawer.getByLabel(/^Gdzie dokładnie/)
        await placement.fill('Test E2E, pierwsze piętro przy windzie')
        await drawer.getByRole('button', { name: 'Aktualizuj i wróć' }).click()

        await expect(drawer).toBeHidden()
        await expect(page.getByText('Zapisano rekord.').first()).toBeVisible()
        const saved = await valueOf<{ placementDetails: string }>(await api.get(`/api/v1/AedDevice/${id}`))
        expect(saved.placementDetails).toBe('Test E2E, pierwsze piętro przy windzie')
    })

    test('a new record without the required fields shows the errors under the fields', async ({ page }) => {
        await page.goto('/pl/panel/aeds')
        await page.waitForLoadState('networkidle')
        const drawer = page.getByRole('dialog', { name: 'Nowy defibrylator' })
        await expect(async () => {
            await page.getByRole('link', { name: 'Dodaj defibrylator' }).first().click()
            await expect(drawer).toBeVisible({ timeout: 2_000 })
        }).toPass({ timeout: 15_000 })
        await expect(drawer.getByText('Pola oznaczone gwiazdką (*) są wymagane.')).toBeVisible()

        await drawer.getByRole('button', { name: 'Utwórz', exact: true }).click()

        for (const label of [/^Nazwa lokalizacji/, /^Adres \(ulica i numer\)/, /^Miejscowość/]) {
            const control = drawer.getByLabel(label)
            await expect(control).toHaveAttribute('aria-invalid', 'true')
            await expect(control).toHaveAccessibleDescription('To pole jest wymagane.')
        }
        // The focus goes to the first wrong field.
        await expect(drawer.getByLabel(/^Nazwa lokalizacji/)).toBeFocused()

        // Bad formats are checked too, with their own message.
        await drawer.getByLabel(/^Kod pocztowy/).fill('50106')
        await drawer.getByRole('button', { name: 'Utwórz', exact: true }).click()
        await expect(drawer.getByLabel(/^Kod pocztowy/)).toHaveAccessibleDescription('Kod pocztowy w formacie 00-000.')
    })

    test('the selection toolbar deletes the selected record after confirmation', async ({ page }) => {
        const name = uniqueName('aed')
        const id = await createAed(api, name)

        const row = await findInList(page, name)
        await row.getByRole('checkbox', { name: `Zaznacz rekord ${name}` }).check()

        const toolbar = page.getByRole('toolbar', { name: 'Akcje dla zaznaczonych rekordów' })
            .or(page.getByRole('region', { name: 'Akcje dla zaznaczonych rekordów' }))
        await expect(toolbar.getByText('Wybrano 1 rekord')).toBeVisible()
        await toolbar.getByRole('button', { name: 'Usuń' }).click()

        const confirm = page.getByRole('alertdialog').or(page.getByRole('dialog', { name: 'Usunąć rekord?' }))
        await expect(confirm).toBeVisible()
        await confirm.getByRole('button', { name: 'Usuń' }).click()

        await expect(page.getByText('Usunięto 1 rekord.')).toBeVisible()
        await expect(page.getByRole('row').filter({ hasText: name })).toHaveCount(0)
        const response = await api.get(`/api/v1/AedDevice/${id}`)
        expect(response.status()).toBe(404)
    })
})
