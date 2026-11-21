'use client'

import { useTranslations } from 'next-intl'
import { Link, usePathname } from '@/i18n/navigation'

const LINKS = [
    { href: '/', key: 'nav.map' },
    { href: '/open-data', key: 'nav.openData' },
] as const

/** Main navigation: the map and the open data page. */
const MainNav = () => {
    const t = useTranslations('common')
    const pathname = usePathname()

    return (
        <nav aria-label={t('nav.label')} className="flex items-center gap-1">
            {LINKS.map(({ href, key }) => {
                const current = href === '/' ? pathname === '/' : pathname.startsWith(href)
                return (
                    <Link
                        key={href}
                        href={href}
                        aria-current={current ? 'page' : undefined}
                        className="rounded-md px-2 py-1.5 text-sm font-semibold text-gray-700 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary aria-[current=page]:text-primary aria-[current=page]:underline aria-[current=page]:underline-offset-4 sm:px-3 dark:text-gray-200 dark:hover:bg-gray-800 dark:aria-[current=page]:text-[#5bd98c]"
                    >
                        {t(key)}
                    </Link>
                )
            })}
        </nav>
    )
}

export default MainNav
