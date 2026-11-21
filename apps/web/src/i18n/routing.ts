import { defineRouting } from 'next-intl/routing'
import { DEFAULT_UI_LOCALE, SUPPORTED_UI_LOCALES } from '@/i18n/supportedLocales'

/**
 * Every page address starts with its language: /pl, /en. The language of an address without one is
 * chosen in src/proxy.ts (cookie → Accept-Language → Polish), as in Car Reminder.
 */
export const routing = defineRouting({
    locales: SUPPORTED_UI_LOCALES,
    defaultLocale: DEFAULT_UI_LOCALE,
    localePrefix: 'always',
    localeDetection: false,
    localeCookie: false,
})
