import { NextResponse } from 'next/server'
import { apiBaseUrl } from '@/server/session'

/** The CSV template of the import (API api/v1/AedImport/template.csv) on the own origin. */
export async function GET() {
    try {
        const response = await fetch(`${apiBaseUrl()}/api/v1/AedImport/template.csv`, { cache: 'no-store' })
        return new NextResponse(await response.arrayBuffer(), {
            status: response.status,
            headers: {
                'Content-Type': response.headers.get('content-type') ?? 'text/csv; charset=utf-8',
                'Content-Disposition': response.headers.get('content-disposition') ?? 'attachment; filename=szablon-importu-aed.csv',
            },
        })
    } catch {
        return NextResponse.json({ error: 'The API is unavailable.' }, { status: 503 })
    }
}
