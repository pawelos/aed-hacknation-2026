import type { Metadata } from 'next'
import { redirect } from 'next/navigation'
import { getTranslations, setRequestLocale } from 'next-intl/server'
import { Link } from '@/i18n/navigation'
import { getCurrentUser } from '@/server/session'
import { getDemoAccounts } from '@/server/demoAccounts'
import SignInForm from './SignInForm'

export async function generateMetadata({ params }: { params: Promise<{ locale: string }> }): Promise<Metadata> {
    const { locale } = await params
    const t = await getTranslations({ locale, namespace: 'auth' })
    return { title: t('metaTitle'), robots: { index: false } }
}

/**
 * Sign-in of the holder panel. The "Zaloguj" button of the landing page (hacknation-2026.pl) links here.
 */
export default async function SignInPage({
    params,
    searchParams,
}: {
    params: Promise<{ locale: string }>
    searchParams: Promise<{ next?: string }>
}) {
    const { locale } = await params
    setRequestLocale(locale)
    const { next } = await searchParams
    if (await getCurrentUser()) {
        redirect(`/${locale}/panel`)
    }
    const t = await getTranslations('auth')
    const demoAccounts = getDemoAccounts()

    return (
        <main id="main" className="flex min-h-0 flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-950">
            <div className="m-auto w-full max-w-md px-4 py-10">
                <div className="flex flex-col gap-5 rounded-2xl border border-gray-200 bg-white p-6 shadow-sm sm:p-8 dark:border-gray-800 dark:bg-gray-900">
                    <div className="flex flex-col gap-2">
                        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{t('title')}</h1>
                        <p className="text-sm text-gray-700 dark:text-gray-300">{t('intro')}</p>
                    </div>
                    <SignInForm next={next ?? ''} demoAccounts={demoAccounts} />
                    <p className="text-xs text-gray-600 dark:text-gray-400">
                        {demoAccounts.length > 0 ? t('quickFill.hint') : t('demo')}
                    </p>
                </div>
                <p className="mt-4 text-center text-sm">
                    <Link href="/" className="text-[#1a56db] underline dark:text-[#8ab4ff]">
                        {t('back')}
                    </Link>
                </p>
            </div>
        </main>
    )
}
