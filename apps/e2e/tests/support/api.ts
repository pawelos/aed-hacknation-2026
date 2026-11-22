import { request, type APIRequestContext, type APIResponse } from '@playwright/test'
import { ACCOUNTS, type Role } from './accounts'

/** API address (the web app's API_URL). */
export const API_URL = process.env.E2E_API_URL || 'http://localhost:5296'

/** Prefix of every record a test creates, so leftovers are easy to find and remove. */
export const E2E_PREFIX = 'e2e-'

/** A unique record name, e.g. `e2e-aed-1795307091000`. */
export const uniqueName = (kind: string) => `${E2E_PREFIX}${kind}-${Date.now()}`

type ApiResult<T> = { isSuccess: boolean; value: T | null }

/**
 * API client signed in as the role, for preparing and removing data where the UI is not what the test checks.
 * Call `dispose()` when done.
 */
export async function apiAs(role: Role): Promise<APIRequestContext> {
    const anonymous = await request.newContext({ baseURL: API_URL })
    const response = await anonymous.post('/api/v1/Auth/login', { data: ACCOUNTS[role] })
    if (!response.ok()) {
        throw new Error(`API sign-in as ${role} failed: ${response.status()} ${await response.text()}`)
    }
    const body = (await response.json()) as ApiResult<{ accessToken: string }>
    await anonymous.dispose()
    return request.newContext({
        baseURL: API_URL,
        extraHTTPHeaders: { Authorization: `Bearer ${body.value?.accessToken}` },
    })
}

/** `value` of an API `Result<T>` response; fails with the response body when it is not a success. */
export async function valueOf<T>(response: APIResponse): Promise<T> {
    const text = await response.text()
    if (!response.ok()) {
        throw new Error(`${response.url()} → ${response.status()} ${text}`)
    }
    return (JSON.parse(text) as ApiResult<T>).value as T
}

type DataSource = { id: string; name: string }

/** First data source the account may write to (the Wrocław holder has exactly one). */
export async function firstDataSource(api: APIRequestContext): Promise<DataSource> {
    const sources = await valueOf<DataSource[]>(await api.get('/api/v1/DataSource'))
    if (sources.length === 0) {
        throw new Error('The account has no data source.')
    }
    return sources[0]
}

/** Creates a working AED available around the clock in the centre of Wrocław; returns its id. */
export async function createAed(api: APIRequestContext, locationName: string): Promise<string> {
    const source = await firstDataSource(api)
    return valueOf<string>(
        await api.post('/api/v1/AedDevice/create', {
            data: {
                dataSourceId: source.id,
                sourceRecordId: locationName,
                locationName,
                address: 'Rynek 1',
                postalCode: '50-106',
                city: 'Wrocław',
                municipality: 'Wrocław',
                municipalityTeryt: '0264011',
                county: 'Wrocław',
                voivodeship: 'LowerSilesian',
                latitude: 51.10995,
                longitude: 17.03215,
                placementDetails: 'Test E2E, parter',
                isIndoor: true,
                facilityType: 'Other',
                availability: 'Public24h',
                availabilityHours: '24/7',
                accessNotes: '',
                status: 'Operational',
                contactPhone: '',
            },
        }),
    )
}

type AedRow = { id: string; locationName: string }

/** Removes AEDs left by test runs (name starting with `e2e-`). */
export async function removeE2eAeds(api: APIRequestContext) {
    const page = await valueOf<{ items: AedRow[] }>(
        await api.get('/api/v1/AedDevice', { params: { search: E2E_PREFIX, pageSize: 100 } }),
    )
    const ids = page.items.filter((a) => a.locationName.startsWith(E2E_PREFIX)).map((a) => a.id)
    if (ids.length > 0) {
        await valueOf(await api.post('/api/v1/AedDevice/batch-delete', { data: { ids } }))
    }
}
