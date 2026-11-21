'use client'

import { useSyncExternalStore } from 'react'
import { useTranslations } from 'next-intl'
import { HiOutlineMoon, HiOutlineSun } from 'react-icons/hi'
import { THEME_COOKIE } from './theme'

const DARK_QUERY = '(prefers-color-scheme: dark)'

const subscribe = (onChange: () => void) => {
    const observer = new MutationObserver(onChange)
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['class'] })
    const media = window.matchMedia(DARK_QUERY)
    media.addEventListener('change', onChange)
    return () => {
        observer.disconnect()
        media.removeEventListener('change', onChange)
    }
}

/** Theme on screen: the chosen class, else the system preference. */
const isDark = () => {
    const classes = document.documentElement.classList
    if (classes.contains('dark')) {
        return true
    }
    if (classes.contains('light')) {
        return false
    }
    return window.matchMedia(DARK_QUERY).matches
}

/** Switches between the light and the dark theme and remembers the choice in a cookie for a year. */
const ThemeToggle = () => {
    const t = useTranslations('common')
    const dark = useSyncExternalStore(subscribe, isDark, () => false)

    const toggle = () => {
        const next = dark ? 'light' : 'dark'
        const classes = document.documentElement.classList
        classes.remove('light', 'dark')
        classes.add(next)
        document.cookie = `${THEME_COOKIE}=${next}; path=/; max-age=31536000; samesite=lax`
    }

    return (
        <button
            type="button"
            onClick={toggle}
            aria-pressed={dark}
            aria-label={dark ? t('theme.light') : t('theme.dark')}
            title={dark ? t('theme.light') : t('theme.dark')}
            suppressHydrationWarning
            className="inline-flex h-10 w-10 items-center justify-center rounded-lg text-gray-700 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary dark:text-gray-200 dark:hover:bg-gray-800"
        >
            {dark ? <HiOutlineSun aria-hidden className="text-xl" /> : <HiOutlineMoon aria-hidden className="text-xl" />}
        </button>
    )
}

export default ThemeToggle
