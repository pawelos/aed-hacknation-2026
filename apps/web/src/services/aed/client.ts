import type {
    AedCard,
    AedFilters,
    AedMapResponse,
    AedSearchResponse,
    ApiResult,
    Bounds,
    NearestAed,
} from './types'

/**
 * Browser calls of the public map. The browser talks only to its own origin (CSP `connect-src 'self'`,
 * as in Car Reminder); app/api/aed/[...path]/route.ts forwards them to the API.
 */
async function get<T>(path: string, params: URLSearchParams, signal?: AbortSignal): Promise<ApiResult<T>> {
    const query = params.toString()
    const response = await fetch(`/api/aed/${path}${query ? `?${query}` : ''}`, {
        signal,
        headers: { Accept: 'application/json' },
    })
    try {
        return (await response.json()) as ApiResult<T>
    } catch {
        return {
            value: null,
            isSuccess: false,
            error: { code: 'Network', description: response.statusText },
        }
    }
}

function filterParams(filters: AedFilters): URLSearchParams {
    const params = new URLSearchParams()
    filters.availability.forEach((value) => params.append('availability', value))
    if (filters.facilityType) {
        params.append('facilityType', filters.facilityType)
    }
    if (filters.openNow) {
        params.set('openNow', 'true')
    }
    return params
}

export function fetchMapPoints(bounds: Bounds, filters: AedFilters, signal?: AbortSignal) {
    const params = filterParams(filters)
    params.set('minLat', bounds.minLat.toFixed(6))
    params.set('minLng', bounds.minLng.toFixed(6))
    params.set('maxLat', bounds.maxLat.toFixed(6))
    params.set('maxLng', bounds.maxLng.toFixed(6))
    return get<AedMapResponse>('map', params, signal)
}

export function fetchNearest(latitude: number, longitude: number, openNow: boolean, signal?: AbortSignal) {
    const params = new URLSearchParams({
        latitude: latitude.toFixed(6),
        longitude: longitude.toFixed(6),
        limit: '5',
        radiusKm: '20',
    })
    if (openNow) {
        params.set('openNow', 'true')
    }
    return get<NearestAed[]>('nearest', params, signal)
}

export function searchAeds(q: string, signal?: AbortSignal) {
    return get<AedSearchResponse>('search', new URLSearchParams({ q }), signal)
}

/** Sends a correction report; website is the hidden honeypot field (empty for people). */
export async function sendReport(
    id: string,
    report: { message: string; reporterContact: string; website: string },
    locale: string,
): Promise<ApiResult<null>> {
    const response = await fetch(`/api/aed/${encodeURIComponent(id)}/report`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json', 'Accept-Language': locale },
        body: JSON.stringify(report),
    })
    try {
        return (await response.json()) as ApiResult<null>
    } catch {
        return { value: null, isSuccess: false, error: { code: 'Network', description: response.statusText } }
    }
}

export function fetchCard(id: string, signal?: AbortSignal) {
    return get<AedCard>(encodeURIComponent(id), new URLSearchParams(), signal)
}
