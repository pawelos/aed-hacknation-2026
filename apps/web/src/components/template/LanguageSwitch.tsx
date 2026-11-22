'use client'

import { useEffect, useRef, useState } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import { useSearchParams } from 'next/navigation'
import { HiCheck, HiChevronDown, HiOutlineSearch } from 'react-icons/hi'
import { Link, usePathname } from '@/i18n/navigation'
import { SUPPORTED_UI_LOCALES, type SupportedUiLocale } from '@/i18n/supportedLocales'

/** Round flags, the same SVGs as on the landing page (site/public/index.html). */
const Flag = ({ code, small = false }: { code: SupportedUiLocale; small?: boolean }) => (
    <span
        aria-hidden
        className={
            'block shrink-0 overflow-hidden rounded-full ring-1 ring-gray-300 dark:ring-gray-600 ' +
            (small ? 'h-[1.125rem] w-[1.125rem]' : 'h-6 w-6')
        }
    >
        {code === 'pl' ? (
            <svg viewBox="0 0 20 20" className="h-full w-full">
                <rect width="20" height="10" fill="#ffffff" />
                <rect y="10" width="20" height="10" fill="#dc143c" />
            </svg>
        ) : (
            <svg viewBox="0 0 60 60" className="h-full w-full">
                <rect width="60" height="60" fill="#012169" />
                <path d="M0 0l60 60M60 0 0 60" stroke="#fff" strokeWidth="12" />
                <path d="M0 0l60 60M60 0 0 60" stroke="#c8102e" strokeWidth="5" />
                <path d="M30 0v60M0 30h60" stroke="#fff" strokeWidth="18" />
                <path d="M30 0v60M0 30h60" stroke="#c8102e" strokeWidth="10" />
            </svg>
        )}
    </span>
)

/** Closes an open dropdown; after Escape the focus goes back to its button. */
const closeDetails = (details: HTMLDetailsElement | null, focusSummary: boolean) => {
    if (!details?.open) {
        return
    }
    details.open = false
    if (focusSummary) {
        details.querySelector('summary')?.focus()
    }
}

/**
 * Language dropdown like on the landing page: flag (+ name on wide screens), a search box and the list with a tick on
 * the current language. Native <details>: opens and closes without JS; the links keep the query (e.g. map filters).
 */
const LanguageSwitch = () => {
    const t = useTranslations('common')
    const locale = useLocale() as SupportedUiLocale
    const pathname = usePathname()
    const searchParams = useSearchParams()
    const query = Object.fromEntries(searchParams.entries())

    const detailsRef = useRef<HTMLDetailsElement>(null)
    const inputRef = useRef<HTMLInputElement>(null)
    const listRef = useRef<HTMLUListElement>(null)
    const [search, setSearch] = useState('')

    const needle = search.trim().toLowerCase()
    const shown = SUPPORTED_UI_LOCALES.filter(
        (code) => !needle || `${t(`language.${code}`)} ${code}`.toLowerCase().includes(needle),
    )

    // Escape and a click outside close the list (as site.js on the landing page).
    useEffect(() => {
        const onClick = (event: MouseEvent) => {
            if (detailsRef.current && !detailsRef.current.contains(event.target as Node)) {
                closeDetails(detailsRef.current, false)
            }
        }
        const onKey = (event: KeyboardEvent) => {
            if (event.key === 'Escape') {
                closeDetails(detailsRef.current, true)
            }
        }
        document.addEventListener('click', onClick)
        document.addEventListener('keydown', onKey)
        return () => {
            document.removeEventListener('click', onClick)
            document.removeEventListener('keydown', onKey)
        }
    }, [])

    const onToggle = () => {
        if (detailsRef.current?.open) {
            setSearch('')
            inputRef.current?.focus()
        }
    }

    return (
        <details ref={detailsRef} onToggle={onToggle} className="relative">
            <summary
                aria-label={`${t('language.label')}: ${t(`language.${locale}`)}`}
                title={t('language.label')}
                className="flex h-10 cursor-pointer list-none items-center gap-1.5 rounded-full px-2 text-sm font-medium text-gray-900 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary dark:text-white dark:hover:bg-gray-800 [&::-webkit-details-marker]:hidden"
            >
                <Flag code={locale} />
                <span className="hidden lg:inline">{t(`language.${locale}`)}</span>
                <HiChevronDown aria-hidden className="hidden h-4 w-4 lg:block" />
            </summary>
            <div className="absolute top-[calc(100%+0.5rem)] right-0 z-50 w-60 max-w-[calc(100vw-1.5rem)] rounded-xl border border-gray-200 bg-white p-2 shadow-lg dark:border-gray-700 dark:bg-gray-900">
                <label className="mb-1.5 flex items-center gap-2 rounded-lg border border-gray-300 px-3 text-gray-600 focus-within:border-primary focus-within:ring-1 focus-within:ring-primary dark:border-gray-600 dark:text-gray-300">
                    <HiOutlineSearch aria-hidden className="h-[1.15rem] w-[1.15rem] shrink-0" />
                    <input
                        ref={inputRef}
                        type="search"
                        value={search}
                        onChange={(event) => setSearch(event.target.value)}
                        onKeyDown={(event) => {
                            // Enter opens the first language on the list.
                            if (event.key === 'Enter') {
                                const first = listRef.current?.querySelector('a')
                                if (first) {
                                    event.preventDefault()
                                    first.click()
                                }
                            }
                        }}
                        placeholder={t('language.search')}
                        aria-label={t('language.searchLabel')}
                        autoComplete="off"
                        className="min-h-10 w-full border-0 bg-transparent text-sm text-gray-900 outline-none placeholder:text-gray-600 dark:text-white dark:placeholder:text-gray-300"
                    />
                </label>
                <nav aria-label={t('language.label')}>
                    <ul ref={listRef}>
                        {shown.map((code) => (
                            <li key={code}>
                                <Link
                                    href={{ pathname, query }}
                                    locale={code}
                                    hrefLang={code}
                                    lang={code}
                                    aria-current={code === locale ? 'true' : undefined}
                                    onClick={() => closeDetails(detailsRef.current, false)}
                                    className="flex min-h-10 items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-gray-900 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-offset-[-2px] focus-visible:outline-primary dark:text-white dark:hover:bg-gray-800"
                                >
                                    <Flag code={code} small />
                                    <span>{t(`language.${code}`)}</span>
                                    {code === locale && (
                                        <HiCheck aria-hidden className="ml-auto h-[1.15rem] w-[1.15rem] text-primary dark:text-[#5bd98c]" />
                                    )}
                                </Link>
                            </li>
                        ))}
                    </ul>
                </nav>
                <p role="status" className="px-3 text-sm text-gray-600 empty:hidden dark:text-gray-300">
                    {shown.length === 0 ? t('language.empty') : ''}
                </p>
            </div>
        </details>
    )
}

export default LanguageSwitch
