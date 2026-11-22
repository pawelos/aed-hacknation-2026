import { getTranslations, setRequestLocale } from 'next-intl/server'
import PageHeader from '@/components/panel/PageHeader'
import { apiFetch } from '@/server/session'
import type { AedDetails, AedPage, DataSource } from '@/services/panel/types'
import AedsClient, { type AedListQuery } from './_components/AedsClient'

const PAGE_SIZE = 25

/**
 * Records of the organisation (search, source and "to verify" filters, pages) and the record drawer: `?id=<id>`
 * opens a record for editing, `?new=1` a new one. Everything is in the URL, so the view can be linked.
 */
export default async function AedListPage({
    params,
    searchParams,
}: {
    params: Promise<{ locale: string }>
    searchParams: Promise<AedListQuery>
}) {
    const { locale } = await params
    setRequestLocale(locale)
    const t = await getTranslations('panel')
    const search = await searchParams
    const page = Math.max(1, Number(search.page) || 1)

    const query = new URLSearchParams({
        page: String(page),
        pageSize: String(PAGE_SIZE),
    })
    if (search.q) query.set('search', search.q)
    if (search.source) query.set('dataSourceId', search.source)
    if (search.verify === '1') query.set('needsVerification', 'true')

    const [list, sources, details] = await Promise.all([
        apiFetch<AedPage>(`/api/v1/AedDevice?${query}`),
        apiFetch<DataSource[]>('/api/v1/DataSource'),
        search.id
            ? apiFetch<AedDetails>(
                  `/api/v1/AedDevice/${encodeURIComponent(search.id)}`,
              )
            : Promise.resolve(null),
    ])
    const data = list.value ?? {
        items: [],
        total: 0,
        page: 1,
        pageSize: PAGE_SIZE,
    }
    const pages = Math.max(1, Math.ceil(data.total / PAGE_SIZE))

    return (
        <>
            <PageHeader
                title={t('aeds.title')}
                description={t('aeds.subtitle')}
            />
            <AedsClient
                items={data.items}
                total={data.total}
                page={page}
                pages={pages}
                query={search}
                sources={sources.value ?? []}
                record={
                    details?.ok && details.value ? details.value : undefined
                }
                isNew={!search.id && search.new === '1'}
            />
        </>
    )
}
