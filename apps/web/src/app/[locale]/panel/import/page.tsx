import {
    getFormatter,
    getTranslations,
    setRequestLocale,
} from 'next-intl/server'
import PageHeader from '@/components/panel/PageHeader'
import {
    TABLE_WRAP_CLASS,
    TD_CLASS,
    TEXT_LINK_CLASS,
    TH_CLASS,
    THEAD_CLASS,
    TR_CLASS,
} from '@/components/panel/styles'
import { apiFetch } from '@/server/session'
import type { DataSource, ImportSummary } from '@/services/panel/types'
import ImportForm from './_components/ImportForm'

/** Import of a holder's file: check first (dry run), then import; the history below. */
export default async function ImportPage({
    params,
}: {
    params: Promise<{ locale: string }>
}) {
    const { locale } = await params
    setRequestLocale(locale)
    const t = await getTranslations('panel')
    const format = await getFormatter()
    const [sources, history] = await Promise.all([
        apiFetch<DataSource[]>('/api/v1/DataSource'),
        apiFetch<ImportSummary[]>('/api/v1/AedImport'),
    ])
    const items = history.value ?? []

    return (
        <>
            <PageHeader
                title={t('import.title')}
                description={
                    <>
                        {t('import.intro')}{' '}
                        {/* A file download from a route handler, not a page: a plain link. */}
                        <a
                            href="/api/import-template"
                            download
                            className={TEXT_LINK_CLASS}
                        >
                            {t('import.template')}
                        </a>
                        {' · '}
                        <a
                            href={`/${locale}/open-data#fields`}
                            className={TEXT_LINK_CLASS}
                        >
                            {t('import.docs')}
                        </a>
                    </>
                }
            />

            <ImportForm sources={sources.value ?? []} />

            <section
                aria-labelledby="history-heading"
                className="flex flex-col gap-3"
            >
                <h2
                    id="history-heading"
                    className="text-lg font-bold text-gray-900 dark:text-white"
                >
                    {t('import.history')}
                </h2>
                {items.length === 0 ? (
                    <p className="text-sm text-gray-700 dark:text-gray-300">
                        {t('import.historyEmpty')}
                    </p>
                ) : (
                    // A focusable region, so keyboard users can scroll the table sideways (WCAG 2.1.1).
                    <div
                        role="region"
                        aria-labelledby="history-heading"
                        tabIndex={0}
                        className={TABLE_WRAP_CLASS}
                    >
                        <table className="w-full min-w-[760px] border-collapse text-left text-sm">
                            <thead className={THEAD_CLASS}>
                                <tr className="border-b border-gray-200 dark:border-gray-800">
                                    {[
                                        'date',
                                        'source',
                                        'fileName',
                                        'rows',
                                        'created',
                                        'updated',
                                        'rejected',
                                        'duplicates',
                                    ].map((column) => (
                                        <th
                                            key={column}
                                            scope="col"
                                            className={TH_CLASS}
                                        >
                                            {column === 'source'
                                                ? t('aeds.source')
                                                : t(`import.${column}`)}
                                        </th>
                                    ))}
                                </tr>
                            </thead>
                            <tbody>
                                {items.map((item) => (
                                    <tr key={item.id} className={TR_CLASS}>
                                        <td className={TD_CLASS}>
                                            {format.dateTime(
                                                new Date(item.createdAt),
                                                {
                                                    dateStyle: 'short',
                                                    timeStyle: 'short',
                                                },
                                            )}
                                        </td>
                                        <td className={TD_CLASS}>
                                            {item.dataSourceName}
                                        </td>
                                        <td
                                            className={`${TD_CLASS} max-w-[16rem] truncate`}
                                            title={item.fileName}
                                        >
                                            {item.format} · {item.fileName}
                                        </td>
                                        <td
                                            className={`${TD_CLASS} tabular-nums`}
                                        >
                                            {item.totalRows}
                                        </td>
                                        <td
                                            className={`${TD_CLASS} tabular-nums`}
                                        >
                                            {item.created}
                                        </td>
                                        <td
                                            className={`${TD_CLASS} tabular-nums`}
                                        >
                                            {item.updated}
                                        </td>
                                        <td
                                            className={`${TD_CLASS} tabular-nums`}
                                        >
                                            {item.rejected}
                                        </td>
                                        <td
                                            className={`${TD_CLASS} tabular-nums`}
                                        >
                                            {item.possibleDuplicates}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </section>
        </>
    )
}
