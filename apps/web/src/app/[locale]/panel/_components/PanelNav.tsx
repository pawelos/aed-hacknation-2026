'use client'

import { useTranslations } from 'next-intl'
import { HiOutlineLogout } from 'react-icons/hi'
import { Link, usePathname } from '@/i18n/navigation'
import { logoutAction } from '@/server/actions/auth'

const LINKS = [
    { href: '/panel', key: 'dashboard' },
    { href: '/panel/aeds', key: 'aeds' },
    { href: '/panel/import', key: 'import' },
    { href: '/panel/reports', key: 'reports' },
] as const

/** Panel menu with the signed-in user and sign-out. */
const PanelNav = ({
    locale,
    userName,
    organisation,
    isAdmin,
}: {
    locale: string
    userName: string
    organisation: string | null
    isAdmin: boolean
}) => {
    const t = useTranslations('panel')
    const pathname = usePathname()

    return (
        <div className="flex flex-col gap-2 border-b border-gray-200 bg-white px-4 py-2 sm:flex-row sm:items-center sm:justify-between sm:px-6 dark:border-gray-800 dark:bg-gray-900">
            <nav
                aria-label={t('nav.label')}
                className="-mx-1 flex gap-1 overflow-x-auto"
            >
                {LINKS.map(({ href, key }) => {
                    const current =
                        href === '/panel'
                            ? pathname === href
                            : pathname.startsWith(href)
                    return (
                        <Link
                            key={href}
                            href={href}
                            aria-current={current ? 'page' : undefined}
                            className="shrink-0 rounded-md px-3 py-1.5 text-sm font-semibold text-gray-700 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-primary aria-[current=page]:bg-primary-subtle aria-[current=page]:text-primary dark:text-gray-200 dark:hover:bg-gray-800 dark:aria-[current=page]:text-[#5bd98c]"
                        >
                            {t(`nav.${key}`)}
                        </Link>
                    )
                })}
            </nav>
            <div className="flex items-center justify-between gap-3 text-sm">
                <span className="text-gray-700 dark:text-gray-300">
                    <span className="font-semibold text-gray-900 dark:text-white">
                        {userName}
                    </span>
                    {' · '}
                    {isAdmin ? t('nav.admin') : organisation}
                </span>
                <form action={logoutAction.bind(null, locale)}>
                    <button
                        type="submit"
                        className="inline-flex items-center gap-1 rounded-md px-2 py-1 font-semibold text-gray-700 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-primary dark:text-gray-200 dark:hover:bg-gray-800"
                    >
                        <HiOutlineLogout aria-hidden /> {t('nav.signOut')}
                    </button>
                </form>
            </div>
        </div>
    )
}

export default PanelNav
