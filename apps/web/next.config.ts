import { NextConfig } from 'next'
import createNextIntlPlugin from 'next-intl/plugin'

const nextConfig: NextConfig = {
    output: 'standalone',
    // Traefik compresses responses on the VPS (as for Car Reminder).
    compress: false,
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
