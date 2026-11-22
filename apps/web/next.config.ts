import { NextConfig } from 'next'
import createNextIntlPlugin from 'next-intl/plugin'

const nextConfig: NextConfig = {
    output: 'standalone',
    // Traefik compresses responses on the VPS (as for Car Reminder).
    compress: false,
    // The import sends the holder's file (API limit 5 MB) in one server action; the default is 1 MB.
    experimental: {
        serverActions: { bodySizeLimit: '6mb' },
        proxyClientMaxBodySize: '6mb',
    },
    // Security headers on every response (CSP with its per-request nonce is set in src/proxy.ts).
    async headers() {
        return [
            {
                source: '/:path*',
                headers: [
                    { key: 'X-Frame-Options', value: 'DENY' },
                    { key: 'X-Content-Type-Options', value: 'nosniff' },
                    {
                        key: 'Referrer-Policy',
                        value: 'strict-origin-when-cross-origin',
                    },
                    // Geolocation on the own origin only: "nearest AED" (Car Reminder blocks it).
                    {
                        key: 'Permissions-Policy',
                        value: 'camera=(), microphone=(), geolocation=(self)',
                    },
                ],
            },
        ]
    },
}

const withNextIntl = createNextIntlPlugin()

export default withNextIntl(nextConfig)
