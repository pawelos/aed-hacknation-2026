import type { ReactNode } from 'react'
import { setRequestLocale } from 'next-intl/server'
import PageMessages from '@/i18n/PageMessages'

export default async function PublicLayout({
    children,
    params,
}: {
    children: ReactNode
    params: Promise<{ locale: string }>
}) {
    // Layouts render in parallel: each one sets the language of the request itself (no next-intl middleware).
    const { locale } = await params
    setRequestLocale(locale)
    return <PageMessages segment="(public)">{children}</PageMessages>
}
