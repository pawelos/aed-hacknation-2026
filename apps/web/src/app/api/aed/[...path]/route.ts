import { NextResponse, type NextRequest } from 'next/server'

/** Address of the API (Aspire sets API_URL; locally the API runs on 5296). */
function apiBaseUrl() {
    return (process.env.API_URL || 'http://localhost:5296').replace(/\/$/, '')
}

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

/** Only the public map endpoints are forwarded: map, nearest, search and the card by id. */
function isAllowed(path: string[]) {
    if (path.length !== 1) {
        return false
    }
    const [segment] = path
    return segment === 'map' || segment === 'nearest' || segment === 'search' || GUID.test(segment)
}

/**
 * Forwards a public map call to the API (`api/v1/PublicAed/...`) with the client address, so the API's
 * per-IP rate limit counts visitors, not the web server.
 */
export async function GET(req: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
    const { path } = await params
    if (!isAllowed(path)) {
        return NextResponse.json({ isSuccess: false, value: null, error: { code: 'NotFound', description: 'Not found.' } }, { status: 404 })
    }

    const forwardedFor = req.headers.get('x-forwarded-for')
    try {
        const response = await fetch(`${apiBaseUrl()}/api/v1/PublicAed/${path[0]}${req.nextUrl.search}`, {
            headers: {
                Accept: 'application/json',
                ...(forwardedFor ? { 'X-Forwarded-For': forwardedFor } : {}),
            },
            cache: 'no-store',
            signal: AbortSignal.timeout(10_000),
        })
        const body = await response.text()
        return new NextResponse(body, {
            status: response.status,
            headers: { 'Content-Type': response.headers.get('Content-Type') ?? 'application/json' },
        })
    } catch {
        return NextResponse.json(
            { isSuccess: false, value: null, error: { code: 'ApiUnavailable', description: 'The API is unavailable.' } },
            { status: 503 },
        )
    }
}

/**
 * Forwards a correction report from the AED card (`<id>/report`) to the API, with the client address (the API
 * allows 10 reports a minute per address) and the page language (validation messages).
 */
export async function POST(req: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
    const { path } = await params
    if (path.length !== 2 || !GUID.test(path[0]) || path[1] !== 'report') {
        return NextResponse.json({ isSuccess: false, value: null, error: { code: 'NotFound', description: 'Not found.' } }, { status: 404 })
    }

    const body = await req.text()
    if (body.length > 10_000) {
        return NextResponse.json({ isSuccess: false, value: null, error: { code: 'TooLarge', description: 'Too large.' } }, { status: 413 })
    }

    const forwardedFor = req.headers.get('x-forwarded-for')
    try {
        const response = await fetch(`${apiBaseUrl()}/api/v1/PublicAed/${path[0]}/report`, {
            method: 'POST',
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
                'Accept-Language': req.headers.get('accept-language') ?? 'pl',
                ...(forwardedFor ? { 'X-Forwarded-For': forwardedFor } : {}),
            },
            body,
            cache: 'no-store',
            signal: AbortSignal.timeout(10_000),
        })
        return new NextResponse(await response.text(), {
            status: response.status,
            headers: { 'Content-Type': response.headers.get('Content-Type') ?? 'application/json' },
        })
    } catch {
        return NextResponse.json(
            { isSuccess: false, value: null, error: { code: 'ApiUnavailable', description: 'The API is unavailable.' } },
            { status: 503 },
        )
    }
}
