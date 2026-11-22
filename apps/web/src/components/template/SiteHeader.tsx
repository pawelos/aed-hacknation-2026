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
            {/* The logo of the landing page (site/public/assets/logo.svg): the same sign on both addresses. */}
            <Link
                href="/"
                className="flex shrink-0 items-center gap-2 rounded-md text-gray-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary dark:text-white"
            >
                {/* eslint-disable-next-line @next/next/no-img-element -- static SVG, no optimisation needed */}
                <img src="/logo.svg" alt="" width={36} height={36} className="h-9 w-9" />
                {/* Phones: the sign only, so the navigation fits in 375 px. */}
                <span className="sr-only text-xl leading-none font-extrabold tracking-tight sm:not-sr-only">
                    AED <span className="text-[#00843a] dark:text-[#2bc96a]">Radar</span>
                </span>
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
