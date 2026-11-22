import type { ReactNode } from 'react'
import {
    getFormatter,
    getTranslations,
    setRequestLocale,
} from 'next-intl/server'
import {
    HiOutlineClipboardCheck,
    HiOutlineClock,
    HiOutlineDatabase,
    HiOutlineExclamation,
    HiOutlineInbox,
    HiOutlinePlus,
    HiOutlineUpload,
} from 'react-icons/hi'
import { Link } from '@/i18n/navigation'
import PageHeader from '@/components/panel/PageHeader'
import {
    CARD_CLASS,
    GROUP_LABEL_CLASS,
    LINK_BUTTON_DEFAULT_CLASS,
    LINK_BUTTON_PRIMARY_CLASS,
} from '@/components/panel/styles'
import { apiFetch } from '@/server/session'
import type { DataSource, PanelSummary } from '@/services/panel/types'

type Tile = {
    label: string
    value: string
    icon: ReactNode
    href?: string
    attention?: boolean
}

/** Panel home: the numbers that need attention, quick actions and the user's sources. */
export default async function PanelHome({
    params,
}: {
    params: Promise<{ locale: string }>
}) {
    const { locale } = await params
    setRequestLocale(locale)
    const t = await getTranslations('panel')
    const format = await getFormatter()
    const [summary, sources] = await Promise.all([
        apiFetch<PanelSummary>('/api/v1/AedDevice/summary'),
        apiFetch<DataSource[]>('/api/v1/DataSource'),
    ])
    const s = summary.value

    const tiles: Tile[] = s
        ? [
              {
                  label: t('dashboard.records'),
                  value: format.number(s.records),
                  icon: <HiOutlineDatabase />,
                  href: '/panel/aeds',
              },
              {
                  label: t('dashboard.needsVerification'),
                  value: format.number(s.needsVerification),
                  icon: <HiOutlineClipboardCheck />,
                  href: '/panel/aeds?verify=1',
                  attention: s.needsVerification > 0,
              },
              {
                  label: t('dashboard.newReports'),
                  value: format.number(s.newReports),
                  icon: <HiOutlineInbox />,
                  href: '/panel/reports',
                  attention: s.newReports > 0,
              },
              {
                  label: t('dashboard.stale'),
                  value: format.number(s.verificationStale),
                  icon: <HiOutlineClock />,
                  attention: s.verificationStale > 0,
              },
              {
                  label: t('dashboard.lastImport'),
                  value: s.lastImportAt
                      ? format.dateTime(new Date(s.lastImportAt), {
                            dateStyle: 'medium',
                            timeStyle: 'short',
                        })
                      : t('dashboard.never'),
                  icon: <HiOutlineUpload />,
                  href: '/panel/import',
              },
          ]
        : []

    return (
        <>
            <PageHeader
                title={t('dashboard.title')}
                description={t('dashboard.subtitle')}
                actions={
                    <Link
                        href="/panel/aeds?new=1"
                        className={LINK_BUTTON_PRIMARY_CLASS}
                    >
                        <HiOutlinePlus aria-hidden className="text-lg" />
                        {t('dashboard.add')}
                    </Link>
                }
            />

            <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-5">
                {tiles.map((tile) => {
                    const content = (
                        <>
                            <span className="flex items-center justify-between gap-2">
                                <span className={GROUP_LABEL_CLASS}>
                                    {tile.label}
                                </span>
                                <span
                                    aria-hidden
                                    className="text-xl text-gray-500 dark:text-gray-400"
                                >
                                    {tile.icon}
                                </span>
                            </span>
                            <span className="mt-2 block text-2xl font-bold text-gray-900 dark:text-white">
                                {tile.value}
                            </span>
                            {/* Attention is shown with an icon and words, not with colour alone (WCAG 1.4.1). */}
                            {tile.attention && (
                                <span className="mt-2 inline-flex items-center gap-1 rounded-full bg-amber-50 px-2 py-0.5 text-xs font-semibold text-amber-900 dark:bg-amber-500/20 dark:text-amber-100">
                                    <HiOutlineExclamation aria-hidden />
                                    {t('dashboard.attention')}
                                </span>
                            )}
                        </>
                    )
                    return (
                        <li
                            key={tile.label}
                            className={`${CARD_CLASS} ${tile.attention ? 'border-amber-300 dark:border-amber-700' : ''}`}
                        >
                            {tile.href ? (
                                <Link
                                    href={tile.href}
                                    className="block h-full rounded-xl p-4 hover:bg-gray-50 focus-visible:outline-2 focus-visible:outline-primary dark:hover:bg-gray-800"
                                >
                                    {content}
                                </Link>
                            ) : (
                                <div className="p-4">{content}</div>
                            )}
                        </li>
                    )
                })}
            </ul>

            <section
                aria-labelledby="actions-heading"
                className="flex flex-col gap-3"
            >
                <h2
                    id="actions-heading"
                    className="text-lg font-bold text-gray-900 dark:text-white"
                >
                    {t('dashboard.actions')}
                </h2>
                <div className="flex flex-wrap gap-2">
                    <Link
                        href="/panel/aeds?verify=1"
                        className={LINK_BUTTON_DEFAULT_CLASS}
                    >
                        <HiOutlineClipboardCheck
                            aria-hidden
                            className="text-lg"
                        />
                        {t('dashboard.goVerify')}
                    </Link>
                    <Link
                        href="/panel/reports"
                        className={LINK_BUTTON_DEFAULT_CLASS}
                    >
                        <HiOutlineInbox aria-hidden className="text-lg" />
                        {t('dashboard.goReports')}
                    </Link>
                    <Link
                        href="/panel/import"
                        className={LINK_BUTTON_DEFAULT_CLASS}
                    >
                        <HiOutlineUpload aria-hidden className="text-lg" />
                        {t('dashboard.goImport')}
                    </Link>
                </div>
            </section>

            <section
                aria-labelledby="sources-heading"
                className="flex flex-col gap-3"
            >
                <h2
                    id="sources-heading"
                    className="text-lg font-bold text-gray-900 dark:text-white"
                >
                    {t('dashboard.sourcesTitle')}
                </h2>
                <ul className="grid gap-3 md:grid-cols-2">
                    {(sources.value ?? []).map((source) => (
                        <li
                            key={source.id}
                            className={`${CARD_CLASS} p-4 text-sm`}
                        >
                            <p className="text-base font-semibold text-gray-900 dark:text-white">
                                {source.name}
                            </p>
                            <p className="text-gray-700 dark:text-gray-300">
                                {source.ownerName}
                            </p>
                            <dl className="mt-3 grid grid-cols-1 gap-2 sm:grid-cols-3">
                                <div>
                                    <dt className={GROUP_LABEL_CLASS}>
                                        {t('dashboard.scope')}
                                    </dt>
                                    <dd className="mt-0.5 text-gray-900 dark:text-gray-100">
                                        {source.referenceScope}
                                    </dd>
                                </div>
                                <div>
                                    <dt className={GROUP_LABEL_CLASS}>
                                        {t('dashboard.license')}
                                    </dt>
                                    <dd className="mt-0.5 text-gray-900 dark:text-gray-100">
                                        {source.license}
                                    </dd>
                                </div>
                                <div>
                                    <dt className={GROUP_LABEL_CLASS}>
                                        {t('dashboard.records')}
                                    </dt>
                                    <dd className="mt-0.5 text-gray-900 dark:text-gray-100">
                                        <Link
                                            href={{
                                                pathname: '/panel/aeds',
                                                query: { source: source.id },
                                            }}
                                            className="underline underline-offset-2 hover:text-primary focus-visible:outline-2 focus-visible:outline-primary"
                                        >
                                            {t('aeds.total', {
                                                count: source.records,
                                            })}
                                        </Link>
                                    </dd>
                                </div>
                            </dl>
                        </li>
                    ))}
                </ul>
            </section>
        </>
    )
}
