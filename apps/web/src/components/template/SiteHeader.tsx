import { Suspense } from 'react'
import { getTranslations } from 'next-intl/server'
import { Link } from '@/i18n/navigation'
import LanguageSwitch from './LanguageSwitch'
import MainNav from './MainNav'
import ThemeToggle from './ThemeToggle'

/** Top bar: name of the action, the emergency number, language and theme. */
const SiteHeader = async () => {
    const t = await getTranslations('common')

    return (
        <header className="flex h-14 shrink-0 items-center justify-between gap-2 border-b border-gray-200 bg-white px-3 sm:px-4 dark:border-gray-800 dark:bg-gray-900">
            <Link
                href="/"
                className="flex items-center gap-2 rounded-md font-bold text-gray-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary dark:text-white"
            >
                <span
                    aria-hidden
                    className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-base text-white"
                >
                    ⚡
                </span>
                {/* Phones: the icon only, so the navigation fits in 375 px. */}
                <span className="sr-only text-base sm:not-sr-only sm:text-lg">{t('appName')}</span>
            </Link>
            <MainNav />
            <div className="ml-auto flex items-center gap-1 sm:gap-2">
                <a
                    href="tel:112"
                    className="hidden rounded-lg bg-[#c8102e] px-3 py-2 text-sm font-bold text-white hover:bg-[#a50d26] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[#c8102e] sm:inline-flex"
                >
                    {t('emergency.call112')}
                </a>
                <Suspense>
                    <LanguageSwitch />
                </Suspense>
                <ThemeToggle />
            </div>
        </header>
    )
}

export default SiteHeader
