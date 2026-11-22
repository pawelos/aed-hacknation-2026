import { expect, test, type APIRequestContext } from '@playwright/test'
import { storageStateOf } from './support/accounts'
import { apiAs, createAed, removeE2eAeds, uniqueName } from './support/api'

/**
 * The path across both sides: a citizen reports a correction from the AED card on the map (no sign-in),
 * the holder sees it in the panel and rejects it.
 */
test.describe('Correction report: map → panel', { tag: '@critical' }, () => {
    let api: APIRequestContext
    const name = uniqueName('report')
    const message = `Szafka jest zamknięta na klucz (${name}).`

    test.beforeAll(async () => {
        api = await apiAs('holder')
        await removeE2eAeds(api)
        await createAed(api, name)
    })

    test.afterAll(async () => {
        await removeE2eAeds(api)
        await api.dispose()
    })

    test('a citizen sends a report from the AED card', async ({ page }) => {
        await page.goto('/pl')
        await page.waitForLoadState('networkidle')
        const searchBox = page.getByRole('searchbox', { name: 'Szukaj miejscowości, gminy lub adresu' })
        await searchBox.fill(name)
        await page.getByRole('search').getByRole('button', { name: 'Szukaj' }).click()

        // One result: the map flies to it and opens its card.
        const card = page.getByRole('dialog')
        await expect(card).toBeVisible({ timeout: 20_000 })
        await expect(card.getByText(name).first()).toBeVisible()

        await card.getByRole('button', { name: 'Zgłoś poprawkę' }).click()
        const form = card.getByRole('form', { name: 'Zgłoś poprawkę' })
        await form.getByRole('button', { name: 'Wyślij zgłoszenie' }).click()
        const text = form.getByLabel(/^Co jest nie tak\?/)
        await expect(text).toHaveAttribute('aria-invalid', 'true')
        await expect(text).toHaveAccessibleDescription('Opisz problem w co najmniej 10 znakach.')

        await text.fill(message)
        await form.getByRole('button', { name: 'Wyślij zgłoszenie' }).click()
        await expect(card.getByText('Dziękujemy! Zgłoszenie trafiło do dysponenta.')).toBeVisible()
    })

    test.describe('holder', () => {
        test.use({ storageState: storageStateOf('holder') })

        test('the holder sees the report in the panel and rejects it', async ({ page }) => {
            await page.goto('/pl/panel/reports')
            await expect(page.getByRole('heading', { level: 1, name: 'Zgłoszenia poprawek' })).toBeVisible()
            const report = page.getByRole('article').or(page.getByRole('region')).filter({ hasText: message })
            await expect(report).toHaveCount(1)
            await expect(report.getByText('Nowe', { exact: true })).toBeVisible()

            await page.waitForLoadState('networkidle')
            await report.getByLabel(/^Notatka/).fill('Klucz jest w portierni, opis w rekordzie jest poprawny.')
            await report.getByRole('button', { name: 'Odrzuć' }).click()

            await expect(report.getByText('Odrzucone', { exact: true })).toBeVisible()
            await expect(report.getByText(/Notatka: Klucz jest w portierni/)).toBeVisible()
        })
    })
})
