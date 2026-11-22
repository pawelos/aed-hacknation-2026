import {
    getFormatter,
    getTranslations,
    setRequestLocale,
} from 'next-intl/server'
import { HiOutlineExternalLink } from 'react-icons/hi'
import { Link } from '@/i18n/navigation'
import PageHeader from '@/components/panel/PageHeader'
import { CARD_CLASS } from '@/components/panel/styles'
import Tag from '@/components/ui/Tag'
import { apiFetch } from '@/server/session'
import type { CorrectionReport } from '@/services/panel/types'
import ResolveReport from './_components/ResolveReport'

/** Colours of the report state tag (text 4.5:1 on its background, also in dark mode). */
function statusTagClass(status: CorrectionReport['status']): string {
    switch (status) {
        case 'New':
            return 'border-0 bg-amber-50 text-amber-900 dark:bg-amber-500/20 dark:text-amber-100'
        case 'Accepted':
            return 'border-0 bg-emerald-50 text-emerald-800 dark:bg-emerald-500/20 dark:text-emerald-200'
        case 'Rejected':
            return 'border-0 bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-200'
    }
}

/** Corrections reported from the map: new ones to accept or reject, then the resolved ones. */
export default async function ReportsPage({
    params,
}: {
    params: Promise<{ locale: string }>
}) {
    const { locale } = await params
    setRequestLocale(locale)
    const t = await getTranslations('panel')
    const format = await getFormatter()
    const response = await apiFetch<CorrectionReport[]>(
        '/api/v1/CorrectionReport',
    )
    const reports = response.value ?? []
    const newCount = reports.filter((report) => report.status === 'New').length

    return (
        <>
            <PageHeader
                title={t('reports.title')}
                description={t('reports.intro')}
            />

            <p className="text-sm text-gray-700 dark:text-gray-300">
                {t('reports.count', { count: reports.length })} ·{' '}
                {t('reports.newCount', { count: newCount })}
            </p>

            {reports.length === 0 ? (
                <p
                    className={`${CARD_CLASS} p-6 text-center text-sm text-gray-700 dark:text-gray-300`}
                >
                    {t('reports.empty')}
                </p>
            ) : (
                <ul className="flex flex-col gap-3">
                    {reports.map((report) => {
                        const headingId = `report-${report.id}`
                        return (
                            <li key={report.id}>
                                <article
                                    aria-labelledby={headingId}
                                    className={`${CARD_CLASS} flex flex-col gap-3 p-4 text-sm sm:p-5`}
                                >
                                    <div className="flex flex-wrap items-start justify-between gap-2">
                                        <div className="min-w-0">
                                            <h2
                                                id={headingId}
                                                className="text-base font-semibold text-gray-900 dark:text-white"
                                            >
                                                {report.locationName}
                                            </h2>
                                            <p className="text-gray-700 dark:text-gray-300">
                                                {report.address}, {report.city}{' '}
                                                ·{' '}
                                                {t('reports.reported', {
                                                    date: format.dateTime(
                                                        new Date(
                                                            report.createdAt,
                                                        ),
                                                        {
                                                            dateStyle: 'medium',
                                                            timeStyle: 'short',
                                                        },
                                                    ),
                                                })}
                                            </p>
                                        </div>
                                        <Tag
                                            className={statusTagClass(
                                                report.status,
                                            )}
                                        >
                                            {t(`reports.${report.status}`)}
                                        </Tag>
                                    </div>
                                    <blockquote className="rounded-lg border-l-4 border-gray-400 bg-gray-50 px-4 py-3 text-gray-900 dark:border-gray-500 dark:bg-gray-800/60 dark:text-gray-100">
                                        {report.message}
                                    </blockquote>
                                    {report.reporterContact && (
                                        <p className="text-gray-700 dark:text-gray-300">
                                            {t('reports.contact', {
                                                contact: report.reporterContact,
                                            })}
                                        </p>
                                    )}
                                    {report.status !== 'New' &&
                                        report.resolutionNote && (
                                            <p className="text-gray-700 dark:text-gray-300">
                                                {t('reports.resolvedNote', {
                                                    note: report.resolutionNote,
                                                })}
                                            </p>
                                        )}
                                    <div className="flex flex-col gap-3 border-t border-gray-200 pt-3 dark:border-gray-700">
                                        {report.status === 'New' && (
                                            <ResolveReport id={report.id} />
                                        )}
                                        <Link
                                            href={{
                                                pathname: '/panel/aeds',
                                                query: {
                                                    id: report.aedDeviceId,
                                                },
                                            }}
                                            className="inline-flex min-h-11 items-center gap-1 self-start font-semibold text-[#1a56db] underline underline-offset-2 focus-visible:outline-2 focus-visible:outline-primary dark:text-[#8ab4ff]"
                                        >
                                            {t('reports.openRecord')}
                                            <span className="sr-only">
                                                : {report.locationName}
                                            </span>
                                            <HiOutlineExternalLink
                                                aria-hidden
                                            />
                                        </Link>
                                    </div>
                                </article>
                            </li>
                        )
                    })}
                </ul>
            )}
        </>
    )
}
