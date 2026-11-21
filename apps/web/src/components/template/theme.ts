/**
 * Cookie with the theme chosen by the visitor: "light" or "dark"; missing = the system preference.
 * The layout puts it on <html> as a class on the server, so the page is painted in the right theme
 * without any script (Car Reminder keeps the theme in a cookie too).
 */
export const THEME_COOKIE = 'aed-theme'

export type ThemeChoice = 'light' | 'dark'

/** Class of <html> for the cookie value; undefined = follow the system. */
export function themeClass(value: string | undefined): ThemeChoice | undefined {
    return value === 'dark' || value === 'light' ? value : undefined
}
