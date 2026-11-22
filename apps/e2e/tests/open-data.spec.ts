import { expect, test } from '@playwright/test'

/**
 * Open data for dane.gov.pl: the page and the files under fixed addresses of the web app
 * (`/api/open-data/<file>`, the same files as `/api/v1/OpenData/<file>` of the API).
 */
test.describe('Open data (dane.gov.pl)', { tag: '@smoke' }, () => {
    test('page shows the data set, the downloads and the sources', async ({ page }) => {
        await page.goto('/pl/open-data')
        await expect(page.getByRole('heading', { level: 1, name: 'Otwarte dane o defibrylatorach AED' })).toBeVisible()
        await expect(page.getByText('Nie udało się wczytać metadanych zbioru.')).toHaveCount(0)
        await expect(page.getByRole('heading', { name: 'Pobierz dane' })).toBeVisible()
        for (const format of ['CSV', 'GeoJSON', 'JSON']) {
            await expect(page.getByRole('link', { name: `Pobierz ${format}`, exact: true })).toBeVisible()
        }
        await expect(page.getByRole('heading', { name: 'Źródła w zbiorze' })).toBeVisible()
    })

    test('CSV has the reference columns and one row per AED', async ({ request }) => {
        const response = await request.get('/api/open-data/aed.csv')
        expect(response.ok()).toBeTruthy()
        expect(response.headers()['content-type']).toContain('text/csv')
        const lines = (await response.text()).replace(/^﻿/, '').trim().split(/\r?\n/)
        const header = lines[0]
        for (const column of ['szerokosc_geograficzna', 'dlugosc_geograficzna', 'licencja_danych']) {
            expect(header).toContain(column)
        }
        expect(lines.length).toBeGreaterThan(10)
    })

    test('GeoJSON is a FeatureCollection of points; the voivodeship filter narrows it', async ({ request }) => {
        const all = await (await request.get('/api/open-data/aed.geojson')).json()
        expect(all.type).toBe('FeatureCollection')
        expect(all.features.length).toBeGreaterThan(10)
        expect(all.features[0].geometry.type).toBe('Point')

        // 02 = dolnośląskie (Wrocław and the surroundings in the demo data).
        const lowerSilesia = await (await request.get('/api/open-data/aed.geojson?wojewodztwo=02')).json()
        expect(lowerSilesia.features.length).toBeGreaterThan(0)
        expect(lowerSilesia.features.length).toBeLessThan(all.features.length)
    })

    test('metadata and the DCAT-AP-PL catalogue are published', async ({ request }) => {
        const metadata = await request.get('/api/open-data/metadata.json')
        expect(metadata.ok()).toBeTruthy()
        expect(JSON.stringify(await metadata.json())).toContain('AED')

        const catalog = await request.get('/api/open-data/catalog.rdf')
        expect(catalog.ok()).toBeTruthy()
        const rdf = await catalog.text()
        expect(rdf).toContain('dcat:Catalog')
        expect(rdf).toContain('dcat:Distribution')
    })
})
