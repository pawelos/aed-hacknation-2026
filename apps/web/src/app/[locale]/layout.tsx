import { getMessages, getTranslations, setRequestLocale } from 'next-intl/server'
import { hasLocale, NextIntlClientProvider } from 'next-intl'
import { notFound } from 'next/navigation'
import type { Metadata } from 'next'
import type { ReactNode } from 'react'
import { Geist } from 'next/font/google'
import { routing } from '@/i18n/routing'
import { CLIENT_NAMESPACES, pickMessages } from '@/i18n/clientNamespaces'
import { cookies } from 'next/headers'
import classNames from 'classnames'
import { THEME_COOKIE, themeClass } from '@/components/template/theme'
import SiteHeader from '@/components/template/SiteHeader'
import UiConfig from '@/components/template/UiConfig'
import '@/assets/styles/app.css'

// Self-hosted at build time, so the CSP needs no Google Fonts origins; latin-ext covers Polish.
const geist = Geist({
    subsets: ['latin', 'latin-ext'],
    weight: ['400', '500', '600', '700'],
    display: 'swap',
    variable: '--font-geist',
})

export async function generateMetadata({
    params,
}: {
    params: Promise<{ locale: string }>
}): Promise<Metadata> {
    const { locale } = await params
    const t = await getTranslations({ locale, namespace: 'common' })
    return {
        title: t('meta.title'),
        description: t('meta.description'),
    }
}

export function generateStaticParams() {
    return routing.locales.map((locale) => ({ locale }))
}

export default async function LocaleLayout({
    children,
    params,
}: Readonly<{
    children: ReactNode
    params: Promise<{ locale: string }>
}>) {
    const { locale } = await params
    if (!hasLocale(routing.locales, locale)) {
        notFound()
    }
    setRequestLocale(locale)
    const messages = await getMessages()
    const t = await getTranslations('common')
    // Chosen theme as a class, set on the server: no script, no flash. Without a choice the CSS follows the system.
    const theme = themeClass((await cookies()).get(THEME_COOKIE)?.value)

    return (
        <html lang={locale} className={classNames(geist.variable, theme)}>
            <body className="flex h-dvh flex-col overflow-hidden font-sans">
                <a
                    href="#main"
                    className="sr-only z-[2000] rounded-md bg-primary px-4 py-2 font-semibold text-white focus:not-sr-only focus:absolute focus:left-2 focus:top-2"
                >
                    {t('skipToContent')}
                </a>
                <NextIntlClientProvider
                    locale={locale}
                    messages={pickMessages(messages, CLIENT_NAMESPACES[''])}
                >
                    <UiConfig locale={locale}>
                        <SiteHeader />
                        {children}
                    </UiConfig>
                </NextIntlClientProvider>
            </body>
        </html>
    )
}
