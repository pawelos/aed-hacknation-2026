'use client'

import { useLocale, useTranslations } from 'next-intl'
import { useSearchParams } from 'next/navigation'
import { Link, usePathname } from '@/i18n/navigation'
import { SUPPORTED_UI_LOCALES } from '@/i18n/supportedLocales'

/** Links to the same page in the other languages (the query, e.g. map filters, is kept). */
const LanguageSwitch = () => {
    const t = useTranslations('common')
    const locale = useLocale()
    const pathname = usePathname()
    const searchParams = useSearchParams()
    const query = Object.fromEntries(searchParams.entries())

    return (
        <nav aria-label={t('language.label')} className="flex items-center gap-1">
            {SUPPORTED_UI_LOCALES.map((code) => (
                <Link
                    key={code}
                    href={{ pathname, query }}
                    locale={code}
                    lang={code}
                    aria-current={code === locale ? 'true' : undefined}
                    className={
                        'rounded-md px-2 py-1 text-sm font-semibold uppercase focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary ' +
                        (code === locale
                            ? 'bg-primary text-white'
                            : 'text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-800')
                    }
                >
                    <span className="sr-only">{t(`language.${code}`)}</span>
                    <span aria-hidden>{code}</span>
                </Link>
            ))}
        </nav>
    )
}

export default LanguageSwitch
