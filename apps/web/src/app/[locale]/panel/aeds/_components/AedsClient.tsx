'use client'

import {
    useEffect,
    useId,
    useRef,
    useState,
    useTransition,
    type FormEvent,
} from 'react'
import { useFormatter, useTranslations } from 'next-intl'
import {
    HiOutlineArrowsExpand,
    HiOutlineCheckCircle,
    HiOutlineExclamation,
    HiOutlineFilter,
    HiOutlinePencilAlt,
    HiOutlineSearch,
    HiOutlineTrash,
} from 'react-icons/hi'
import { Link, useRouter } from '@/i18n/navigation'
import BulkActionBar from '@/components/panel/BulkActionBar'
import ConfirmDialog from '@/components/panel/ConfirmDialog'
import LookupSelect from '@/components/panel/LookupSelect'
import {
    INPUT_CLASS,
    NOTICE_ERROR_CLASS,
    NOTICE_SUCCESS_CLASS,
} from '@/components/panel/styles'
import Button from '@/components/ui/Button'
import Tag from '@/components/ui/Tag'
import Tooltip from '@/components/ui/Tooltip'
import { INPUT_LIMITS } from '@/constants/input-limits.constant'
import {
    batchDeleteAedsAction,
    batchVerifyAedsAction,
} from '@/server/actions/panel'
import type { AedStatus } from '@/services/aed/types'
import type {
    AedDetails,
    AedListItem,
    DataSource,
} from '@/services/panel/types'
import AedDrawer from './AedDrawer'

/** Query of the list page; empty values are left out of the URL. */
export type AedListQuery = {
    q?: string
    source?: string
    verify?: string
    page?: string
    saved?: string
    deleted?: string
    id?: string
    new?: string
}

type Props = {
    items: AedListItem[]
    total: number
    page: number
    pages: number
    query: AedListQuery
    sources: DataSource[]
    /** The record named by `?id=` (its drawer is open). */
    record?: AedDetails
    /** `?new=1`: the drawer of a new record is open. */
    isNew: boolean
}

const PATHNAME = '/panel/aeds'

/** Colours of the state tag (text 4.5:1 on its background, also in dark mode). */
function statusTagClass(status: AedStatus): string {
    switch (status) {
        case 'Operational':
            return 'border-0 bg-emerald-50 text-emerald-800 dark:bg-emerald-500/20 dark:text-emerald-200'
        case 'ToVerify':
        case 'Outdated':
            return 'border-0 bg-amber-50 text-amber-900 dark:bg-amber-500/20 dark:text-amber-100'
        case 'Withdrawn':
            return 'border-0 bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-200'
    }
}

/** Page numbers around the current one, with gaps (null) – e.g. 1 … 4 5 6 … 12. */
function pageNumbers(page: number, pages: number): (number | null)[] {
    const result: (number | null)[] = []
    for (let n = 1; n <= pages; n++) {
        if (n === 1 || n === pages || Math.abs(n - page) <= 1) {
            result.push(n)
        } else if (result[result.length - 1] !== null) {
            result.push(null)
        }
    }
    return result
}

/**
 * AED records in the style of Car Reminder's vehicles: search and filters above, a table with selection, the
 * selection toolbar at the bottom (edit, confirm, delete) and the record drawer. The filters, the page and the open
 * record live in the URL (`?id=` / `?new=1`), so every view can be reloaded or shared.
 */
const AedsClient = ({
    items,
    total,
    page,
    pages,
    query,
    sources,
    record,
    isNew,
}: Props) => {
    const t = useTranslations('panel')
    const tEnums = useTranslations('enums')
    const format = useFormatter()
    const router = useRouter()
    const [navigating, startNavigation] = useTransition()
    const [actionPending, startAction] = useTransition()
    const [search, setSearch] = useState(query.q ?? '')
    // Last text this box sent to the URL, and the URL text last seen.
    const sentSearch = useRef(query.q ?? '')
    const [seenQuery, setSeenQuery] = useState(query.q ?? '')
    // The URL text changed without typing here (Back / Forward, a link): the box shows it. A change the box sent
    // itself is skipped, so keys typed while the page loads are not lost.
    const urlSearch = query.q ?? ''
    if (urlSearch !== seenQuery) {
        setSeenQuery(urlSearch)
        if (urlSearch !== sentSearch.current) {
            sentSearch.current = urlSearch
            setSearch(urlSearch)
        }
    }
    const [selected, setSelected] = useState<string[]>([])
    const [listExpanded, setListExpanded] = useState(false)
    const [deleteOpen, setDeleteOpen] = useState(false)
    const [notice, setNotice] = useState<{
        text: string
        error?: boolean
    } | null>(null)
    const selectAllRef = useRef<HTMLInputElement>(null)
    const sourceFilterId = useId()

    const flash = query.saved
        ? t('aeds.saved')
        : query.deleted
          ? t('aeds.deleted')
          : null
    const shownNotice = notice ?? (flash ? { text: flash } : null)

    /** Link / navigation target: the current query with changes; one-off flags and empty values dropped. */
    const target = (changes: AedListQuery) => {
        const next: Record<string, string> = {}
        Object.entries({
            ...query,
            saved: undefined,
            deleted: undefined,
            ...changes,
        }).forEach(([key, value]) => {
            if (value) next[key] = value
        })
        return { pathname: PATHNAME, query: next }
    }

    const navigate = (changes: AedListQuery, replace = false) => {
        setNotice(null)
        startNavigation(() => {
            if (replace) {
                router.replace(target(changes), { scroll: false })
            } else {
                router.push(target(changes), { scroll: false })
            }
        })
    }

    const sendSearch = (text: string) => {
        sentSearch.current = text.trim()
        navigate({ q: text.trim(), page: undefined }, true)
    }

    // The search runs 400 ms after the last key (Enter runs it at once).
    useEffect(() => {
        if (search.trim() === (query.q ?? '')) return
        const timer = setTimeout(() => sendSearch(search), 400)
        return () => clearTimeout(timer)
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [search])

    const submitSearch = (event: FormEvent) => {
        event.preventDefault()
        sendSearch(search)
    }

    // Selection is kept for the records on this page only.
    const pageIds = items.map((item) => item.id)
    const selectedIds = selected.filter((id) => pageIds.includes(id))
    const allSelected = items.length > 0 && selectedIds.length === items.length
    const someSelected = selectedIds.length > 0 && !allSelected

    useEffect(() => {
        if (selectAllRef.current)
            selectAllRef.current.indeterminate = someSelected
    }, [someSelected])

    const toggle = (id: string, checked: boolean) =>
        setSelected((prev) =>
            checked ? [...prev, id] : prev.filter((x) => x !== id),
        )
    const toggleAll = (checked: boolean) => setSelected(checked ? pageIds : [])

    /**
     * One request for all the chosen records. On success the selection is cleared; on an error (e.g. a record was
     * removed in the meantime) nothing changed, so the selection stays for another try.
     */
    const runBatch = (
        action: typeof batchVerifyAedsAction,
        done: (count: number) => string,
    ) =>
        startAction(async () => {
            const result = await action(selectedIds)
            setDeleteOpen(false)
            if (result.ok) {
                setNotice({ text: done(result.count ?? selectedIds.length) })
                setSelected([])
            } else {
                setNotice({
                    text:
                        result.message ??
                        Object.values(result.fieldErrors ?? {})[0] ??
                        t('form.errorTitle'),
                    error: true,
                })
            }
            router.refresh()
        })

    const verifySelected = () =>
        runBatch(batchVerifyAedsAction, (count) =>
            t('aeds.verifiedMany', { count }),
        )
    const deleteSelected = () =>
        runBatch(batchDeleteAedsAction, (count) =>
            t('aeds.deletedMany', { count }),
        )

    const selectedLabel =
        selectedIds.length === 1
            ? t('aeds.selectedOne')
            : t('aeds.selectedMany', { count: selectedIds.length })
    const iconButton =
        'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary'

    return (
        <div
            className={
                (listExpanded
                    ? 'fixed inset-0 z-30 flex flex-col gap-4 overflow-auto bg-white p-4 sm:p-6 dark:bg-gray-950'
                    : 'flex flex-col gap-4') +
                // Room for the selection toolbar fixed at the bottom.
                (selectedIds.length > 0 ? ' pb-28' : '')
            }
        >
            {/* Toolbar: search on the left, view and actions on the right (as in Car Reminder). */}
            <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                <form
                    role="search"
                    onSubmit={submitSearch}
                    className="relative w-full lg:max-w-md"
                >
                    <label htmlFor="aed-search" className="sr-only">
                        {t('aeds.searchLabel')}
                    </label>
                    <HiOutlineSearch
                        aria-hidden
                        className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-lg text-gray-500"
                    />
                    <input
                        id="aed-search"
                        type="search"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        maxLength={INPUT_LIMITS.search}
                        placeholder={`${t('aeds.search')}: ${t('aeds.searchPlaceholder')}`}
                        className={`${INPUT_CLASS} pl-11`}
                    />
                </form>
                <div className="flex flex-wrap items-center gap-2">
                    <Tooltip
                        title={
                            listExpanded
                                ? t('aeds.exitFullScreen')
                                : t('aeds.fullScreen')
                        }
                    >
                        <Button
                            type="button"
                            variant="ghost"
                            icon={<HiOutlineArrowsExpand aria-hidden />}
                            aria-label={
                                listExpanded
                                    ? t('aeds.exitFullScreen')
                                    : t('aeds.fullScreen')
                            }
                            aria-pressed={listExpanded}
                            className={iconButton}
                            onClick={() => setListExpanded((v) => !v)}
                        />
                    </Tooltip>
                    <span
                        className="flex items-center gap-1 px-1 text-sm text-gray-700 dark:text-gray-300"
                        aria-live="polite"
                    >
                        <HiOutlineFilter aria-hidden />
                        {t('aeds.total', { count: total })}
                    </span>
                    <Link
                        href={target({ new: '1', id: undefined })}
                        scroll={false}
                        className="inline-flex min-h-11 items-center rounded-lg bg-primary px-5 text-sm font-semibold text-white hover:bg-primary-deep focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                    >
                        {t('aeds.add')}
                    </Link>
                </div>
            </div>

            <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-center">
                <div className="flex flex-col gap-1 sm:flex-row sm:items-center sm:gap-2">
                    <label
                        htmlFor={sourceFilterId}
                        className="text-sm font-semibold text-gray-800 dark:text-gray-200"
                    >
                        {t('aeds.source')}
                    </label>
                    <div className="sm:w-80">
                        <LookupSelect
                            label={t('aeds.source')}
                            fieldProps={{
                                id: sourceFilterId,
                                'aria-invalid': false,
                            }}
                            value={query.source ?? ''}
                            options={sources.map((source) => ({
                                value: source.id,
                                label: source.name,
                                extra: source.ownerName,
                            }))}
                            extraHeader={t('form.colOwner')}
                            placeholder={t('aeds.allSources')}
                            clearable
                            onChange={(next) =>
                                navigate({
                                    source: next || undefined,
                                    page: undefined,
                                })
                            }
                        />
                    </div>
                </div>
                <label className="flex min-h-11 cursor-pointer items-center gap-3 text-sm text-gray-800 dark:text-gray-200">
                    <input
                        type="checkbox"
                        checked={query.verify === '1'}
                        onChange={(e) =>
                            navigate({
                                verify: e.target.checked ? '1' : undefined,
                                page: undefined,
                            })
                        }
                        className="h-5 w-5 accent-[#00843a] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                    />
                    {t('aeds.onlyToVerify')}
                </label>
            </div>

            <p
                role="status"
                className={
                    shownNotice
                        ? shownNotice.error
                            ? NOTICE_ERROR_CLASS
                            : NOTICE_SUCCESS_CLASS
                        : 'sr-only'
                }
            >
                {shownNotice?.text}
            </p>

            {items.length === 0 ? (
                <p className="rounded-xl border border-gray-200 bg-white p-6 text-center text-sm text-gray-700 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-300">
                    {t('aeds.empty')}
                </p>
            ) : (
                // A focusable region, so keyboard users can scroll the table sideways on narrow screens (WCAG 2.1.1).
                <div
                    role="region"
                    aria-label={t('aeds.tableCaption')}
                    tabIndex={0}
                    aria-busy={navigating || actionPending}
                    className={`overflow-x-auto rounded-xl border border-gray-200 bg-white focus-visible:outline-2 focus-visible:outline-primary dark:border-gray-800 dark:bg-gray-900 ${navigating ? 'opacity-70' : ''}`}
                >
                    <table className="w-full min-w-[760px] border-collapse text-left text-sm">
                        <caption className="sr-only">
                            {t('aeds.tableCaption')}
                        </caption>
                        <thead className="bg-gray-50 dark:bg-gray-800/60">
                            <tr className="border-b border-gray-200 dark:border-gray-800">
                                <th scope="col" className="w-14 px-4 py-3">
                                    <input
                                        ref={selectAllRef}
                                        type="checkbox"
                                        checked={allSelected}
                                        onChange={(e) =>
                                            toggleAll(e.target.checked)
                                        }
                                        aria-label={t('aeds.selectAll')}
                                        className="h-5 w-5 cursor-pointer accent-[#00843a] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                                    />
                                </th>
                                {[
                                    t('aeds.name'),
                                    t('aeds.source'),
                                    t('aeds.status'),
                                    t('aeds.availability'),
                                    t('aeds.verified'),
                                ].map((label) => (
                                    <th
                                        key={label}
                                        scope="col"
                                        className="px-4 py-3 text-xs font-semibold uppercase tracking-wider text-gray-700 dark:text-gray-300"
                                    >
                                        {label}
                                    </th>
                                ))}
                            </tr>
                        </thead>
                        <tbody>
                            {items.map((item) => {
                                const isSelected = selectedIds.includes(item.id)
                                return (
                                    <tr
                                        key={item.id}
                                        className={`border-b border-gray-100 align-top last:border-0 dark:border-gray-800 ${isSelected ? 'bg-primary-subtle' : 'hover:bg-gray-50 dark:hover:bg-gray-800/40'}`}
                                    >
                                        <td className="px-4 py-3">
                                            <input
                                                type="checkbox"
                                                checked={isSelected}
                                                onChange={(e) =>
                                                    toggle(
                                                        item.id,
                                                        e.target.checked,
                                                    )
                                                }
                                                aria-label={t(
                                                    'aeds.selectRow',
                                                    { name: item.locationName },
                                                )}
                                                className="mt-0.5 h-5 w-5 cursor-pointer accent-[#00843a] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                                            />
                                        </td>
                                        <th
                                            scope="row"
                                            className="px-4 py-3 font-normal"
                                        >
                                            <Link
                                                href={target({
                                                    id: item.id,
                                                    new: undefined,
                                                })}
                                                scroll={false}
                                                className="font-semibold text-gray-900 underline decoration-gray-400 underline-offset-4 hover:text-primary hover:decoration-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary dark:text-white"
                                            >
                                                {item.locationName}
                                            </Link>
                                            <span className="block text-gray-700 dark:text-gray-300">
                                                {item.address}, {item.city}
                                                {item.sourceRecordId && (
                                                    <span className="text-gray-600 dark:text-gray-400">
                                                        {' '}
                                                        · {item.sourceRecordId}
                                                    </span>
                                                )}
                                            </span>
                                            {(item.needsVerification ||
                                                item.newReports > 0) && (
                                                <span className="mt-1 inline-flex flex-wrap items-center gap-2 text-xs font-semibold text-[#8a5100] dark:text-[#ffc56b]">
                                                    <HiOutlineExclamation
                                                        aria-hidden
                                                    />
                                                    {item.needsVerification &&
                                                        t('aeds.toVerify')}
                                                    {item.newReports > 0 &&
                                                        t('aeds.reports', {
                                                            count: item.newReports,
                                                        })}
                                                </span>
                                            )}
                                        </th>
                                        <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                                            {item.dataSourceName}
                                        </td>
                                        <td className="px-4 py-3">
                                            <Tag
                                                className={statusTagClass(
                                                    item.status,
                                                )}
                                            >
                                                {tEnums(
                                                    `status.${item.status}`,
                                                )}
                                            </Tag>
                                        </td>
                                        <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                                            {tEnums(
                                                `availability.${item.availability}`,
                                            )}
                                        </td>
                                        <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                                            {item.lastVerifiedAt
                                                ? format.dateTime(
                                                      new Date(
                                                          item.lastVerifiedAt,
                                                      ),
                                                      { dateStyle: 'medium' },
                                                  )
                                                : t('aeds.never')}
                                        </td>
                                    </tr>
                                )
                            })}
                        </tbody>
                    </table>
                </div>
            )}

            {pages > 1 && (
                <nav
                    aria-label={t('aeds.pagination')}
                    className="flex flex-wrap items-center justify-center gap-1 sm:justify-start"
                >
                    {page > 1 && (
                        <Link
                            href={target({ page: String(page - 1) })}
                            className="inline-flex min-h-11 items-center rounded-lg px-3 text-sm text-gray-800 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-primary dark:text-gray-200 dark:hover:bg-gray-800"
                        >
                            ‹ {t('aeds.prev')}
                        </Link>
                    )}
                    {pageNumbers(page, pages).map((n, index) =>
                        n === null ? (
                            <span
                                key={`gap-${index}`}
                                aria-hidden
                                className="px-2 text-gray-500"
                            >
                                …
                            </span>
                        ) : (
                            <Link
                                key={n}
                                href={target({ page: String(n) })}
                                aria-label={t('aeds.pageNumber', { page: n })}
                                aria-current={n === page ? 'page' : undefined}
                                className={`inline-flex h-11 min-w-11 items-center justify-center rounded-lg border text-sm focus-visible:outline-2 focus-visible:outline-primary ${n === page ? 'border-gray-500 font-semibold text-gray-900 dark:text-white' : 'border-transparent text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800'}`}
                            >
                                {n}
                            </Link>
                        ),
                    )}
                    {page < pages && (
                        <Link
                            href={target({ page: String(page + 1) })}
                            className="inline-flex min-h-11 items-center rounded-lg px-3 text-sm text-gray-800 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-primary dark:text-gray-200 dark:hover:bg-gray-800"
                        >
                            {t('aeds.next')} ›
                        </Link>
                    )}
                </nav>
            )}

            <BulkActionBar
                count={selectedIds.length}
                label={selectedLabel}
                ariaLabel={t('aeds.selectionActions')}
            >
                {selectedIds.length === 1 && (
                    <Button
                        type="button"
                        size="sm"
                        variant="edit"
                        icon={<HiOutlinePencilAlt aria-hidden />}
                        onClick={() =>
                            navigate({ id: selectedIds[0], new: undefined })
                        }
                        disabled={actionPending}
                    >
                        {t('aeds.edit')}
                    </Button>
                )}
                <Button
                    type="button"
                    size="sm"
                    icon={<HiOutlineCheckCircle aria-hidden />}
                    onClick={verifySelected}
                    loading={actionPending}
                >
                    {t('aeds.verifySelected')}
                </Button>
                <Button
                    type="button"
                    size="sm"
                    variant="danger"
                    icon={<HiOutlineTrash aria-hidden />}
                    onClick={() => setDeleteOpen(true)}
                    disabled={actionPending}
                >
                    {t('aeds.delete')}
                </Button>
            </BulkActionBar>

            <ConfirmDialog
                isOpen={deleteOpen}
                title={
                    selectedIds.length > 1
                        ? t('form.deleteTitleMany')
                        : t('form.deleteTitleOne')
                }
                confirmText={t('aeds.delete')}
                cancelText={t('form.cancel')}
                closeLabel={t('form.close')}
                danger
                pending={actionPending}
                onConfirm={deleteSelected}
                onCancel={() => setDeleteOpen(false)}
            >
                <p>
                    {selectedIds.length > 1
                        ? t('form.deleteConfirmMany', {
                              count: selectedIds.length,
                          })
                        : t('form.confirmDelete')}
                </p>
            </ConfirmDialog>

            <AedDrawer
                open={!!record || isNew}
                record={record}
                sources={sources}
                onClose={(result) =>
                    navigate({
                        id: undefined,
                        new: undefined,
                        saved: result === 'saved' ? '1' : undefined,
                        deleted: result === 'deleted' ? '1' : undefined,
                    })
                }
                onCreated={(id) => navigate({ id, new: undefined }, true)}
                onChanged={() => router.refresh()}
            />
        </div>
    )
}

export default AedsClient
