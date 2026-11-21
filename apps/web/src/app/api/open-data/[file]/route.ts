import { NextResponse, type NextRequest } from 'next/server'

/** Address of the API (Aspire sets API_URL; locally the API runs on 5296). */
function apiBaseUrl() {
    return (process.env.API_URL || 'http://localhost:5296').replace(/\/$/, '')
}

/** Files of the open data set; the public addresses are {web}/api/open-data/<file> (stable, linked from dane.gov.pl). */
const FILES = new Set([
    'aed.csv',
    'aed.geojson',
    'aed.json',
    'metadata.json',
    'catalog.rdf',
    'wzorcowy-zakres-danych-aed.xlsx',
])

/** Response headers passed on from the API. */
const PASSED_HEADERS = [
    'content-type',
    'content-disposition',
    'cache-control',
    'last-modified',
    'access-control-allow-origin',
    'content-security-policy',
]

/** Forwards a download to the API (`api/v1/OpenData/<file>`) with its query (filters) and headers. */
export async function GET(req: NextRequest, { params }: { params: Promise<{ file: string }> }) {
    const { file } = await params
    if (!FILES.has(file)) {
        return NextResponse.json({ error: 'Not found.' }, { status: 404 })
    }

    try {
        const response = await fetch(`${apiBaseUrl()}/api/v1/OpenData/${file}${req.nextUrl.search}`, {
            cache: 'no-store',
            signal: AbortSignal.timeout(30_000),
            headers: req.headers.get('x-forwarded-for')
                ? { 'X-Forwarded-For': req.headers.get('x-forwarded-for')! }
                : undefined,
        })
        const headers = new Headers()
        PASSED_HEADERS.forEach((name) => {
            const value = response.headers.get(name)
            if (value) {
                headers.set(name, value)
            }
        })
        return new NextResponse(await response.arrayBuffer(), { status: response.status, headers })
    } catch {
        return NextResponse.json({ error: 'The API is unavailable.' }, { status: 503 })
    }
}
