import { NextResponse, type NextRequest } from 'next/server'
import {
    buildContentSecurityPolicy,
    CSP_NONCE_HEADER,
} from '@/constants/csp.constant'
import {
    DEFAULT_UI_LOCALE,
    LOCALE_COOKIE,
    isSupportedUiLocale,
    type SupportedUiLocale,
} from '@/i18n/supportedLocales'

/** First supported language of the Accept-Language header, by quality. */
function fromAcceptLanguage(header: string | null): SupportedUiLocale | null {
    if (!header) {
        return null
    }
    const ranked = header
        .split(',')
        .map((part) => {
            const [tag, ...params] = part.trim().split(';')
            const q = params.find((p) => p.trim().startsWith('q='))
            return {
                language: tag.toLowerCase().split('-')[0],
                quality: q ? Number(q.trim().slice(2)) || 0 : 1,
            }
        })
        .sort((a, b) => b.quality - a.quality)
    return (
        ranked.map((r) => r.language).find(isSupportedUiLocale) ?? null
    )
}

/**
 * Language prefix and CSP (pattern of Car Reminder, apps/web/src/proxy.ts, without sign-in and maintenance).
 * An address without a language is redirected to cookie → Accept-Language → Polish. Every page gets a
 * per-request nonce; Next.js reads it from the request CSP header and adds it to its own scripts.
 */
export function proxy(req: NextRequest) {
    const { pathname, search } = req.nextUrl
    const first = pathname.split('/')[1]

    if (!isSupportedUiLocale(first)) {
        const cookie = req.cookies.get(LOCALE_COOKIE)?.value
        const locale = isSupportedUiLocale(cookie)
            ? cookie
            : (fromAcceptLanguage(req.headers.get('accept-language')) ??
              DEFAULT_UI_LOCALE)
        const url = req.nextUrl.clone()
        url.pathname = `/${locale}${pathname === '/' ? '' : pathname}`
        url.search = search
        return NextResponse.redirect(url)
    }

    const nonce = btoa(crypto.randomUUID())
    const csp = buildContentSecurityPolicy(
        nonce,
        process.env.NODE_ENV === 'development',
    )
    const requestHeaders = new Headers(req.headers)
    requestHeaders.set(CSP_NONCE_HEADER, nonce)
    requestHeaders.set('Content-Security-Policy', csp)

    const response = NextResponse.next({ request: { headers: requestHeaders } })
    response.headers.set('Content-Security-Policy', csp)
    // Remember the language of the address for the next visit without one.
    if (req.cookies.get(LOCALE_COOKIE)?.value !== first) {
        response.cookies.set(LOCALE_COOKIE, first, {
            path: '/',
            maxAge: 60 * 60 * 24 * 365,
            sameSite: 'lax',
        })
    }
    return response
}

export const config = {
    // Pages only: no route handlers (app/api), static assets or files with an extension.
    matcher: ['/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)'],
}
