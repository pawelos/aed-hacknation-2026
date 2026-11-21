import { ReactNode } from 'react'
import { NextIntlClientProvider } from 'next-intl'
import { getLocale, getMessages } from 'next-intl/server'
import {
    CLIENT_NAMESPACES,
    pickMessages,
    type ClientSegment,
} from './clientNamespaces'

/**
 * Gives the client components of a route segment only the namespaces it uses (`CLIENT_NAMESPACES`).
 * Locale, time zone and formats come from the request config.
 */
const PageMessages = async ({
    segment,
    children,
}: {
    segment: ClientSegment
    children: ReactNode
}) => {
    const [locale, messages] = await Promise.all([getLocale(), getMessages()])
    return (
        <NextIntlClientProvider
            locale={locale}
            messages={pickMessages(messages, CLIENT_NAMESPACES[segment])}
        >
            {children}
        </NextIntlClientProvider>
    )
}

export default PageMessages
