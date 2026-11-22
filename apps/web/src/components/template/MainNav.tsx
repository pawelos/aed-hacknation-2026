'use client'

import { useTranslations } from 'next-intl'
import { Link, usePathname } from '@/i18n/navigation'

// short: the label on phones, so the three links, the language and the theme fit in one 375 px row.
const LINKS = [
    { href: '/', key: 'nav.map', short: 'nav.map', match: [] },
    { href: '/open-data', key: 'nav.openData', short: 'nav.openDataShort', match: ['/open-data'] },
    // Without a session the proxy sends it to the sign-in page, which counts as the panel too.
    { href: '/panel', key: 'nav.panel', short: 'nav.panel', match: ['/panel', '/sign-in'] },
] as const

/** Main navigation: the map, the open data page and the holder panel. */
const MainNav = () => {
    const t = useTranslations('common')
    const pathname = usePathname()

    return (
        <nav aria-label={t('nav.label')} className="flex items-center gap-0.5 sm:gap-1">
            {LINKS.map(({ href, key, short, match }) => {
                const current = href === '/' ? pathname === '/' : match.some((prefix) => pathname.startsWith(prefix))
                return (
                    <Link
                        key={href}
                        href={href}
                        aria-current={current ? 'page' : undefined}
                        className="rounded-md px-2 py-1.5 text-sm font-semibold whitespace-nowrap text-gray-700 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary aria-[current=page]:text-primary aria-[current=page]:underline aria-[current=page]:underline-offset-4 sm:px-3 dark:text-gray-200 dark:hover:bg-gray-800 dark:aria-[current=page]:text-[#5bd98c]"
                    >
                        <span className="sm:hidden">{t(short)}</span>
                        <span className="hidden sm:inline">{t(key)}</span>
                    </Link>
                )
            })}
        </nav>
    )
}

export default MainNav
