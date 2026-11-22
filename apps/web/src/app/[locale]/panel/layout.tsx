import type { Metadata } from 'next'
import type { ReactNode } from 'react'
import { redirect } from 'next/navigation'
import { getTranslations, setRequestLocale } from 'next-intl/server'
import PageMessages from '@/i18n/PageMessages'
import { getCurrentUser } from '@/server/session'
import PanelNav from './_components/PanelNav'

export async function generateMetadata({
    params,
}: {
    params: Promise<{ locale: string }>
}): Promise<Metadata> {
    const { locale } = await params
    const t = await getTranslations({ locale, namespace: 'panel' })
    return { title: t('metaTitle'), robots: { index: false } }
}

/**
 * Holder panel: only for a valid session (the proxy sends visitors without the cookie to the sign-in page; an
 * expired token lands here and goes there too).
 */
export default async function PanelLayout({
    children,
    params,
}: {
    children: ReactNode
    params: Promise<{ locale: string }>
}) {
    const { locale } = await params
    setRequestLocale(locale)
    const user = await getCurrentUser()
    if (!user) {
        redirect(`/${locale}/sign-in`)
    }

    return (
        <PageMessages segment="panel">
            <div className="flex min-h-0 flex-1 flex-col bg-gray-50 dark:bg-gray-950">
                <PanelNav
                    locale={locale}
                    userName={user.displayName || user.email}
                    organisation={user.ownerName}
                    isAdmin={user.roles.includes('Admin')}
                />
                <main id="main" className="min-h-0 flex-1 overflow-y-auto">
                    <div className="mx-auto flex max-w-6xl flex-col gap-6 px-4 py-6 sm:px-6">
                        {children}
                    </div>
                </main>
            </div>
        </PageMessages>
    )
}
