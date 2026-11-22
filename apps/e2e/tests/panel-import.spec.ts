import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { expect, test } from '@playwright/test'
import { storageStateOf } from './support/accounts'

/** Example file of the Wrocław holder: 8 rows, some with deliberate mistakes (apps/data, docs/import-danych.md). */
const SAMPLE_CSV = path.join(path.dirname(fileURLToPath(import.meta.url)), '..', '..', 'data', 'import-przyklad-wroclaw.csv')

test.describe('Panel: import', { tag: '@critical' }, () => {
    test.use({ storageState: storageStateOf('holder') })

    test('a check without saving shows the summary and the rows with errors', async ({ page }) => {
        await page.goto('/pl/panel/import')
        await expect(page.getByRole('heading', { level: 1, name: 'Import danych' })).toBeVisible()
        await page.waitForLoadState('networkidle')

        // The holder has one data source, so it is already chosen.
        await page.getByLabel(/^Plik CSV lub GeoJSON/).setInputFiles(SAMPLE_CSV)
        await page.getByRole('button', { name: 'Sprawdź plik (bez zapisu)' }).click()

        const report = page.getByRole('region', { name: /^Raport sprawdzenia: nic nie zapisano/ }).first()
        await expect(report).toBeVisible({ timeout: 20_000 })
        await expect(report.getByRole('heading', { level: 2 })).toContainText('import-przyklad-wroclaw.csv')

        // Summary. Counts that depend only on the file (new / updated / unchanged depend on the database).
        const stat = (term: string) =>
            report.getByRole('term').filter({ hasText: new RegExp(`^${term}$`) }).locator('xpath=following-sibling::*[1]')
        await expect(stat('Wiersze')).toHaveText('8')
        // Rejected on purpose: no location name, a point in Berlin, the same id twice.
        await expect(stat('Odrzucone')).toHaveText('3')

        // The table lists the rows with errors or warnings, each with its message.
        const table = report.getByRole('table')
        await expect(table.getByRole('row').filter({ hasText: 'Brak nazwy lokalizacji.' })).toContainText('Odrzucony')
        await expect(table.getByRole('row').filter({ hasText: 'leży poza Polską' })).toContainText('Odrzucony')
        await expect(table.getByText(/Możliwy duplikat rekordu/).first()).toBeVisible()
        await expect(report.getByText('Pominięte kolumny: Kolor obudowy')).toBeVisible()
    })

    test('without a file the error is under the file field', async ({ page }) => {
        await page.goto('/pl/panel/import')
        await page.waitForLoadState('networkidle')
        await page.getByRole('button', { name: 'Sprawdź plik (bez zapisu)' }).click()
        const file = page.getByLabel(/^Plik CSV lub GeoJSON/)
        await expect(file).toHaveAttribute('aria-invalid', 'true')
        await expect(file).toHaveAccessibleDescription(/Wybierz plik\./)
    })
})
