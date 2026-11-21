import type { Metadata } from 'next'
import type { ReactNode } from 'react'
import { getFormatter, getTranslations, setRequestLocale } from 'next-intl/server'
import { HiOutlineDocumentDownload, HiOutlineExternalLink } from 'react-icons/hi'
import { getDatasetMetadata, type LocalizedText } from '@/services/aed/server'

/** Downloads on the own origin (app/api/open-data/[file]); the metadata gives the public addresses for dane.gov.pl. */
const FILES = '/api/open-data/'

export async function generateMetadata({ params }: { params: Promise<{ locale: string }> }): Promise<Metadata> {
    const { locale } = await params
    const t = await getTranslations({ locale, namespace: 'openData' })
    return { title: t('metaTitle') }
}

const Section = ({ id, title, children }: { id: string; title: string; children: ReactNode }) => (
    <section aria-labelledby={id} className="flex flex-col gap-3">
        <h2 id={id} className="text-xl font-bold text-gray-900 dark:text-white">
            {title}
        </h2>
        {children}
    </section>
)

const DownloadLink = ({ href, label, primary = false }: { href: string; label: string; primary?: boolean }) => (
    <a
        href={href}
        className={
            'inline-flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary ' +
            (primary
                ? 'bg-primary text-white hover:bg-primary-deep'
                : 'border border-gray-300 text-gray-900 hover:bg-gray-100 dark:border-gray-700 dark:text-gray-100 dark:hover:bg-gray-800')
        }
    >
        <HiOutlineDocumentDownload aria-hidden className="text-lg" />
        {label}
    </a>
)

/**
 * "Open data": the AED data set for dane.gov.pl – downloads, metadata, DCAT-AP-PL catalog, the reference
 * data set (fields and dictionaries), the integrated sources and how a holder shares data.
 */
export default async function OpenDataPage({ params }: { params: Promise<{ locale: string }> }) {
    const { locale } = await params
    setRequestLocale(locale)
    const t = await getTranslations('openData')
    const tCommon = await getTranslations('common')
    const format = await getFormatter()
    const metadata = await getDatasetMetadata()
    const text = (value: LocalizedText) => (locale === 'pl' ? value.pl : value.en)

    return (
        <main id="main" className="min-h-0 flex-1 overflow-y-auto bg-white dark:bg-gray-900">
            <div className="mx-auto flex max-w-5xl flex-col gap-10 px-4 py-8 text-gray-800 sm:px-6 dark:text-gray-200">
                <header className="flex flex-col gap-3">
                    <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl dark:text-white">{t('title')}</h1>
                    <p className="max-w-3xl text-base leading-relaxed">{t('intro')}</p>
                </header>

                {!metadata ? (
                    <p role="alert" className="text-[#b3261e] dark:text-[#ff8a80]">
                        {t('unavailable')}
                    </p>
                ) : (
                    <>
                        <dl className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                            {[
                                [t('stats.records'), format.number(metadata.records)],
                                [t('stats.sources'), format.number(metadata.sources.length)],
                                [
                                    t('stats.modified'),
                                    metadata.modified
                                        ? format.dateTime(new Date(metadata.modified), { dateStyle: 'medium' })
                                        : '–',
                                ],
                                [t('stats.license'), metadata.license],
                            ].map(([label, value]) => (
                                <div key={label} className="rounded-xl border border-gray-200 p-4 dark:border-gray-800">
                                    <dt className="text-xs font-semibold uppercase tracking-wide text-gray-600 dark:text-gray-400">
                                        {label}
                                    </dt>
                                    <dd className="mt-1 text-xl font-bold text-gray-900 dark:text-white">{value}</dd>
                                </div>
                            ))}
                        </dl>
                        <p className="-mt-6 text-sm text-gray-700 dark:text-gray-300">{t('licenseNote')}</p>

                        <Section id="download" title={t('download.heading')}>
                            <ul className="grid gap-3 md:grid-cols-3">
                                {metadata.resources.map((resource) => {
                                    const file = resource.downloadUrl.split('/').pop()!
                                    return (
                                        <li
                                            key={resource.format}
                                            className="flex flex-col gap-3 rounded-xl border border-gray-200 p-4 dark:border-gray-800"
                                        >
                                            <h3 className="text-base font-bold text-gray-900 dark:text-white">{text(resource.title)}</h3>
                                            <p className="flex-1 text-sm">{text(resource.description)}</p>
                                            <code className="break-all text-xs text-gray-600 dark:text-gray-400">
                                                {resource.downloadUrl}
                                            </code>
                                            <DownloadLink href={FILES + file} label={t('download.button', { format: resource.format })} primary />
                                        </li>
                                    )
                                })}
                            </ul>
                            <p className="text-sm text-gray-700 dark:text-gray-300">{t('download.filters')}</p>

                            <h3 className="mt-2 text-lg font-bold text-gray-900 dark:text-white">{t('download.docsHeading')}</h3>
                            <ul className="flex flex-col gap-3">
                                {[
                                    ['wzorcowy-zakres-danych-aed.xlsx', t('download.template'), t('download.templateDesc')],
                                    ['metadata.json', t('download.metadata'), t('download.metadataDesc')],
                                    ['catalog.rdf', t('download.dcat'), t('download.dcatDesc')],
                                ].map(([file, label, description]) => (
                                    <li key={file} className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                                        <span className="text-sm">{description}</span>
                                        <DownloadLink href={FILES + file} label={label} />
                                    </li>
                                ))}
                            </ul>
                        </Section>

                        <Section id="how-to" title={t('howTo.heading')}>
                            <ol className="grid gap-3 md:grid-cols-3">
                                {[1, 2, 3].map((step) => (
                                    <li key={step} className="flex gap-3 rounded-xl bg-gray-50 p-4 dark:bg-gray-800">
                                        <span
                                            aria-hidden
                                            className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary font-bold text-white"
                                        >
                                            {step}
                                        </span>
                                        <div>
                                            <h3 className="text-base font-bold text-gray-900 dark:text-white">{t(`howTo.step${step}Title`)}</h3>
                                            <p className="mt-1 text-sm">{t(`howTo.step${step}`)}</p>
                                        </div>
                                    </li>
                                ))}
                            </ol>
                        </Section>

                        <Section id="sources" title={t('sources.heading')}>
                            <div className="overflow-x-auto">
                                <table className="w-full min-w-[640px] border-collapse text-left text-sm">
                                    <thead>
                                        <tr className="border-b border-gray-300 dark:border-gray-700">
                                            {['name', 'owner', 'scope', 'license', 'records', 'verified', 'csv'].map((column) => (
                                                <th key={column} scope="col" className="px-2 py-2 font-semibold text-gray-900 dark:text-white">
                                                    {t(`sources.${column}`)}
                                                </th>
                                            ))}
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {metadata.sources.map((source) => (
                                            <tr key={source.id} className="border-b border-gray-200 align-top dark:border-gray-800">
                                                <th scope="row" className="px-2 py-2 font-medium">
                                                    {source.url ? (
                                                        <a
                                                            href={source.url}
                                                            target="_blank"
                                                            rel="noopener noreferrer"
                                                            className="inline-flex items-center gap-1 text-[#1a56db] underline dark:text-[#8ab4ff]"
                                                        >
                                                            {source.name}
                                                            <HiOutlineExternalLink aria-hidden />
                                                        </a>
                                                    ) : (
                                                        source.name
                                                    )}
                                                </th>
                                                <td className="px-2 py-2">{source.owner}</td>
                                                <td className="px-2 py-2">{source.referenceScope}</td>
                                                <td className="px-2 py-2">
                                                    <a href={source.licenseUrl} className="underline" target="_blank" rel="noopener noreferrer">
                                                        {source.license}
                                                    </a>
                                                </td>
                                                <td className="px-2 py-2 text-right tabular-nums">{source.records}</td>
                                                <td className="px-2 py-2">
                                                    {source.lastVerifiedAt
                                                        ? format.dateTime(new Date(source.lastVerifiedAt), { dateStyle: 'medium' })
                                                        : t('sources.never')}
                                                </td>
                                                <td className="px-2 py-2">
                                                    <a href={`${FILES}aed.csv?zrodlo=${source.id}`} className="underline">
                                                        CSV
                                                    </a>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </Section>

                        <Section id="fields" title={t('fields.heading')}>
                            <p className="text-sm">
                                {t('fields.intro', { count: metadata.fields.length, version: metadata.schemaVersion })}
                            </p>
                            <div className="overflow-x-auto">
                                <table className="w-full min-w-[720px] border-collapse text-left text-sm">
                                    <thead>
                                        <tr className="border-b border-gray-300 dark:border-gray-700">
                                            {['name', 'type', 'required', 'format', 'description'].map((column) => (
                                                <th key={column} scope="col" className="px-2 py-2 font-semibold text-gray-900 dark:text-white">
                                                    {t(`fields.${column}`)}
                                                </th>
                                            ))}
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {metadata.fields.map((field) => (
                                            <tr key={field.name} className="border-b border-gray-200 align-top dark:border-gray-800">
                                                <th scope="row" className="px-2 py-2 font-mono text-xs font-semibold">
                                                    {field.name}
                                                </th>
                                                <td className="px-2 py-2">{field.type}</td>
                                                <td className="px-2 py-2">{field.required ? t('fields.yes') : t('fields.no')}</td>
                                                <td className="px-2 py-2">{field.format}</td>
                                                <td className="px-2 py-2">{text(field.description)}</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </Section>

                        <Section id="dictionaries" title={t('dictionaries.heading')}>
                            <div className="flex flex-col gap-2">
                                {metadata.dictionaries.map((dictionary) => (
                                    <details key={dictionary.name} className="rounded-lg border border-gray-200 dark:border-gray-800">
                                        <summary className="cursor-pointer px-4 py-2 font-mono text-sm font-semibold">
                                            {dictionary.name} ({dictionary.codes.length})
                                        </summary>
                                        <table className="w-full border-collapse text-left text-sm">
                                            <thead>
                                                <tr className="border-y border-gray-200 dark:border-gray-800">
                                                    <th scope="col" className="px-4 py-1">{t('dictionaries.code')}</th>
                                                    <th scope="col" className="px-4 py-1">{t('dictionaries.label')}</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {dictionary.codes.map((code) => (
                                                    <tr key={code.code}>
                                                        <td className="px-4 py-1 font-mono text-xs">{code.code}</td>
                                                        <td className="px-4 py-1">{locale === 'pl' ? code.labelPl : code.labelEn}</td>
                                                    </tr>
                                                ))}
                                            </tbody>
                                        </table>
                                    </details>
                                ))}
                            </div>
                        </Section>

                        <p className="text-sm text-gray-600 dark:text-gray-400">
                            {metadata.publisher} · <a href={`mailto:${metadata.contactEmail}`} className="underline">{metadata.contactEmail}</a>
                            {' · '}
                            {tCommon('appName')}
                        </p>
                    </>
                )}
            </div>
        </main>
    )
}
