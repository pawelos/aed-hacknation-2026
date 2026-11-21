/** Request header through which the proxy passes the per-request CSP nonce to server components. */
export const CSP_NONCE_HEADER = 'x-nonce'

/** Origin of the OpenStreetMap map tiles shown under the AED points. */
export const OSM_TILE_ORIGIN = 'https://tile.openstreetmap.org'

/**
 * Content-Security-Policy of the web app (pattern of Car Reminder, apps/web/src/constants/csp.constant.ts).
 * Scripts run only with the per-request nonce (Next.js adds it to its own scripts; server components can
 * read it from {@link CSP_NONCE_HEADER}), so injected markup cannot run code. Images: own origin and the OSM tiles.
 * The browser talks only to its own origin; the API is reached through the route handlers in app/api.
 */
export function buildContentSecurityPolicy(
    nonce: string,
    isDev: boolean,
): string {
    const directives = [
        "default-src 'self'",
        // 'unsafe-eval': React dev tooling and HMR only.
        `script-src 'self' 'nonce-${nonce}' 'strict-dynamic'${isDev ? " 'unsafe-eval'" : ''}`,
        // Leaflet positions the map tiles and markers with inline styles.
        "style-src 'self' 'unsafe-inline'",
        `img-src 'self' data: blob: ${OSM_TILE_ORIGIN}`,
        "font-src 'self' data:",
        `connect-src 'self'${isDev ? ' ws: wss:' : ''}`,
        "frame-src 'none'",
        "object-src 'none'",
        "base-uri 'self'",
        "form-action 'self'",
        "frame-ancestors 'none'",
    ]
    return directives.join('; ')
}
