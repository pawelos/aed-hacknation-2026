import { expect, test, type Page } from '@playwright/test'

/** Centre of Wrocław: the demo data has a dozen AEDs within a few kilometres. */
const WROCLAW = { latitude: 51.1079, longitude: 17.0385 }

/** Results list next to the map (on a desktop it is always visible). */
const results = (page: Page) => page.getByRole('region', { name: /AED|Najbliższe/ })

/** Opens the map and waits until React has hydrated and the first points are listed. */
async function openMap(page: Page) {
    await page.goto('/pl')
    await expect(page.getByRole('heading', { level: 1, name: 'Mapa defibrylatorów AED' })).toBeAttached()
    await expect(results(page).getByRole('listitem').first()).toBeVisible({ timeout: 20_000 })
}

test.describe('Public map (citizen, no sign-in)', { tag: '@smoke' }, () => {
    test('shows AEDs of Poland with the emergency bar', async ({ page }) => {
        await openMap(page)
        await expect(page.getByText(/112/).first()).toBeVisible()
        await expect(results(page).getByRole('heading', { level: 2 })).toHaveText(/\d+ AED/)
    })

    test('search by town moves the list to the town; a row opens the AED card', async ({ page }) => {
        await openMap(page)
        const searchBox = page.getByRole('searchbox', { name: 'Szukaj miejscowości, gminy lub adresu' })
        await searchBox.fill('Kraków')
        await page.getByRole('search').getByRole('button', { name: 'Szukaj' }).click()

        const firstRow = results(page).getByRole('listitem').filter({ hasText: 'Kraków' }).first()
        await expect(firstRow).toBeVisible({ timeout: 20_000 })
        const name = (await firstRow.locator('span.font-semibold').first().textContent())?.trim() ?? ''
        await firstRow.getByRole('button').click()

        const card = page.getByRole('dialog')
        await expect(card).toBeVisible()
        await expect(card.getByText(name, { exact: true }).first()).toBeVisible()
        await expect(card.getByText('Źródło danych')).toBeVisible()
        await expect(card.getByText('Licencja danych')).toBeVisible()
        await expect(card.getByRole('link', { name: /Nawiguj/ })).toHaveAttribute('href', /^https?:\/\//)
    })

    test('a too short query shows the error under the search field', async ({ page }) => {
        await openMap(page)
        const searchBox = page.getByRole('searchbox', { name: 'Szukaj miejscowości, gminy lub adresu' })
        await searchBox.fill('K')
        await page.getByRole('search').getByRole('button', { name: 'Szukaj' }).click()
        await expect(searchBox).toHaveAttribute('aria-invalid', 'true')
        await expect(searchBox).toHaveAccessibleDescription('Wpisz co najmniej 2 znaki.')
    })

    test('"Najbliższe AED" lists the nearest devices with the distance', async ({ page, context }) => {
        await context.grantPermissions(['geolocation'])
        await context.setGeolocation(WROCLAW)
        await openMap(page)

        await page.getByRole('button', { name: 'Najbliższe AED' }).click()

        await expect(results(page).getByRole('heading', { level: 2, name: 'Najbliższe AED' })).toBeVisible({
            timeout: 20_000,
        })
        const rows = results(page).getByRole('listitem')
        await expect(rows.first()).toContainText('Wrocław')
        await expect(rows.first()).toContainText(/od Ciebie/)
        await expect(page.getByRole('button', { name: 'Pokaż wszystkie' })).toBeVisible()
    })

    test('without the location permission it points to the search', async ({ page, context }) => {
        await context.clearPermissions()
        await openMap(page)
        await page.getByRole('button', { name: 'Najbliższe AED' }).click()
        await expect(page.getByText('Brak dostępu do lokalizacji. Wpisz adres w wyszukiwarce.')).toBeVisible()
    })
})

test.describe('Language dropdown in the header', () => {
    test('opens with the search focused, Escape closes it, English keeps the page and the query', async ({ page }) => {
        await page.goto('/pl/open-data?e2e=1')
        const summary = page.getByLabel('Język: Polski')
        await expect(summary).toBeVisible()

        await summary.click()
        const search = page.getByRole('searchbox', { name: 'Szukaj języka' })
        await expect(search).toBeFocused()
        const languages = page.getByRole('navigation', { name: 'Język' })
        await expect(languages.getByRole('link', { name: 'Polski' })).toHaveAttribute('aria-current', 'true')

        await page.keyboard.press('Escape')
        await expect(search).toBeHidden()
        await expect(summary).toBeFocused()

        await summary.click()
        await search.fill('eng')
        await expect(languages.getByRole('link', { name: 'Polski' })).toHaveCount(0)
        await search.press('Enter')

        await expect(page).toHaveURL(/\/en\/open-data\?e2e=1$/)
        await expect(page.getByLabel('Language: English')).toBeVisible()
    })
})
