import { cache } from 'react'
import { cookies } from 'next/headers'
import { getLocale } from 'next-intl/server'

/** httpOnly cookie with the API access token of the panel (JWT, 12 h). */
export const SESSION_COOKIE = 'aed-session'

/** Address of the API (Aspire sets API_URL; locally 5296). */
export function apiBaseUrl() {
    return (process.env.API_URL || 'http://localhost:5296').replace(/\/$/, '')
}

/** Answer of the API with its HTTP status (the body is the API's Result envelope). */
export type ApiResponse<T> = {
    status: number
    ok: boolean
    value: T | null
    error: { code: string; description: string; fields?: Record<string, string> } | null
}

export async function getSessionToken(): Promise<string | undefined> {
    return (await cookies()).get(SESSION_COOKIE)?.value
}

/**
 * Calls the API on the server with the session token and the page language (validation messages in pl / en).
 * The browser never talks to the API directly (CSP connect-src 'self', token in an httpOnly cookie).
 */
export async function apiFetch<T>(
    path: string,
    init: { method?: string; json?: unknown; form?: FormData; anonymous?: boolean } = {},
): Promise<ApiResponse<T>> {
    const headers: Record<string, string> = { Accept: 'application/json' }
    let locale = 'pl'
    try {
        locale = await getLocale()
    } catch {
        // Outside a localized request (e.g. a route handler): Polish.
    }
    headers['Accept-Language'] = locale
    if (!init.anonymous) {
        const token = await getSessionToken()
        if (token) {
            headers.Authorization = `Bearer ${token}`
        }
    }
    let body: BodyInit | undefined
    if (init.json !== undefined) {
        headers['Content-Type'] = 'application/json'
        body = JSON.stringify(init.json)
    } else if (init.form) {
        body = init.form
    }

    try {
        const response = await fetch(`${apiBaseUrl()}${path}`, {
            method: init.method ?? 'GET',
            headers,
            body,
            cache: 'no-store',
            signal: AbortSignal.timeout(60_000),
        })
        const text = await response.text()
        const parsed = text ? (JSON.parse(text) as { value?: T; error?: ApiResponse<T>['error'] }) : null
        return {
            status: response.status,
            ok: response.ok,
            value: (parsed?.value ?? null) as T | null,
            error: response.ok ? null : (parsed?.error ?? { code: `Http.${response.status}`, description: response.statusText }),
        }
    } catch {
        return { status: 503, ok: false, value: null, error: { code: 'ApiUnavailable', description: 'The API is unavailable.' } }
    }
}

/** Account of the signed-in panel user (once per request); null without a valid session. */
export type PanelUser = {
    userId: string
    email: string
    displayName: string
    roles: string[]
    ownerId: string | null
    ownerName: string | null
}

export const getCurrentUser = cache(async (): Promise<PanelUser | null> => {
    if (!(await getSessionToken())) {
        return null
    }
    const response = await apiFetch<PanelUser>('/api/v1/Auth/me')
    return response.ok ? response.value : null
})
