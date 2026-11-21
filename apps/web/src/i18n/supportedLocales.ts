/**
 * UI languages that have a `messages/<namespace>/<code>.json` file in every namespace folder.
 * AED Radar is Polish first (dane.gov.pl, Polish users) with English for visitors and the jury.
 */
export const SUPPORTED_UI_LOCALES = ['pl', 'en'] as const

export type SupportedUiLocale = (typeof SUPPORTED_UI_LOCALES)[number]

/** Language of an address without one when neither the cookie nor the browser chooses. */
export const DEFAULT_UI_LOCALE: SupportedUiLocale = 'pl'

/** Cookie with the language chosen by the visitor. */
export const LOCALE_COOKIE = 'NEXT_LOCALE'

export const isSupportedUiLocale = (
    value: string | null | undefined,
): value is SupportedUiLocale =>
    !!value && (SUPPORTED_UI_LOCALES as readonly string[]).includes(value)
