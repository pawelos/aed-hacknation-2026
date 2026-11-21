import { getRequestConfig } from 'next-intl/server'
import { MESSAGE_NAMESPACES } from './messageNamespaces'
import { DEFAULT_UI_LOCALE, isSupportedUiLocale } from './supportedLocales'

async function loadNamespace(namespace: string, locale: string) {
    return (await import(`../../messages/${namespace}/${locale}.json`)).default
}

/** All messages of a language, keyed by namespace. */
async function loadMessages(locale: string) {
    const contents = await Promise.all(
        MESSAGE_NAMESPACES.map((namespace) => loadNamespace(namespace, locale)),
    )
    return Object.fromEntries(
        MESSAGE_NAMESPACES.map((namespace, index) => [
            namespace,
            contents[index],
        ]),
    )
}

// The page's language comes from its address (/pl/...); src/proxy.ts adds a missing one.
export default getRequestConfig(async ({ requestLocale }) => {
    const fromPath = await requestLocale
    const locale = isSupportedUiLocale(fromPath) ? fromPath : DEFAULT_UI_LOCALE
    return {
        locale,
        messages: await loadMessages(locale),
        // Opening hours and verification dates are Polish local time.
        timeZone: 'Europe/Warsaw',
    }
})
