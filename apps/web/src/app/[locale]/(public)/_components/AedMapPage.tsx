'use client'

import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import dynamic from 'next/dynamic'
import { useTranslations } from 'next-intl'
import { HiOutlineLocationMarker, HiOutlineFilter } from 'react-icons/hi'
import Button from '@/components/ui/Button'
import Spinner from '@/components/ui/Spinner'
import { fetchMapPoints, fetchNearest, searchAeds } from '@/services/aed/client'
import type { AedMapPoint, Bounds } from '@/services/aed/types'
import AedList, { type AedListItem } from './AedList'
import AedCard from './AedCard'
import AedFiltersPanel from './AedFiltersPanel'
import SearchBox from './SearchBox'
import useAedFilters from './useAedFilters'
import type { FlyTarget } from './AedMap'

// Leaflet needs `window`: the map is rendered in the browser only.
const AedMap = dynamic(() => import('./AedMap'), {
    ssr: false,
    loading: () => (
        <div className="flex h-full items-center justify-center">
            <Spinner size={32} />
        </div>
    ),
})

/** Box around Poland; a view outside it has no devices and is not sent to the API. */
const POLAND = { minLat: 49.0, maxLat: 54.9, minLng: 14.1, maxLng: 24.2 }

function overlapsPoland(b: Bounds) {
    return b.minLat <= POLAND.maxLat && b.maxLat >= POLAND.minLat && b.minLng <= POLAND.maxLng && b.maxLng >= POLAND.minLng
}

function clampBounds(b: Bounds): Bounds {
    return {
        minLat: Math.max(b.minLat, -90),
        maxLat: Math.min(b.maxLat, 90),
        minLng: Math.max(b.minLng, -180),
        maxLng: Math.min(b.maxLng, 180),
    }
}

type Position = { latitude: number; longitude: number }

type Nearest = { origin: Position; items: AedListItem[] }

/**
 * Public map of AEDs: search, filters, "nearest AED", the map with clustered pins, the same results as a
 * list (usable without the map) and the card of the selected device.
 */
const AedMapPage = () => {
    const t = useTranslations('aedMap')
    const tCommon = useTranslations('common')
    const { filters, setFilters } = useAedFilters()

    const [bounds, setBounds] = useState<Bounds | null>(null)
    const [points, setPoints] = useState<AedMapPoint[]>([])
    const [truncated, setTruncated] = useState(false)
    const [loading, setLoading] = useState(false)
    const [loadFailed, setLoadFailed] = useState(false)
    const [selectedId, setSelectedId] = useState<string | null>(null)
    const [flyTo, setFlyTo] = useState<FlyTarget | null>(null)
    const [nearest, setNearest] = useState<Nearest | null>(null)
    const [locating, setLocating] = useState(false)
    const [searching, setSearching] = useState(false)
    const [searchError, setSearchError] = useState<string | null>(null)
    const [notice, setNotice] = useState<string | null>(null)
    const [view, setView] = useState<'map' | 'list'>('map')
    const [filtersOpen, setFiltersOpen] = useState(false)
    const flyKey = useRef(0)

    const moveMap = useCallback((latitude: number, longitude: number, zoom: number) => {
        flyKey.current += 1
        setFlyTo({ latitude, longitude, zoom, key: flyKey.current })
    }, [])

    // Points of the visible area, fetched shortly after the map stops moving.
    useEffect(() => {
        if (!bounds) {
            return
        }
        if (!overlapsPoland(bounds)) {
            setPoints([])
            setTruncated(false)
            return
        }
        const controller = new AbortController()
        const timer = setTimeout(() => {
            setLoading(true)
            fetchMapPoints(clampBounds(bounds), filters, controller.signal)
                .then((result) => {
                    setLoadFailed(!result.isSuccess)
                    setPoints(result.value?.items ?? [])
                    setTruncated(result.value?.truncated ?? false)
                })
                .catch(() => {
                    if (!controller.signal.aborted) {
                        setLoadFailed(true)
                    }
                })
                .finally(() => {
                    if (!controller.signal.aborted) {
                        setLoading(false)
                    }
                })
        }, 250)
        return () => {
            clearTimeout(timer)
            controller.abort()
        }
    }, [bounds, filters])

    const loadNearest = useCallback(
        (origin: Position) => {
            fetchNearest(origin.latitude, origin.longitude, filters.openNow)
                .then((result) => {
                    const items = (result.value ?? []).map((n) => ({ point: n.point, distanceMeters: n.distanceMeters }))
                    setNearest({ origin, items })
                    setNotice(items.length === 0 ? t('nearest.none') : null)
                    // The list with distances first; a row or a pin opens the card.
                    setView('list')
                })
                .catch(() => setNotice(t('list.error')))
                .finally(() => setLocating(false))
        },
        [filters.openNow, t],
    )

    const findNearest = () => {
        if (!('geolocation' in navigator)) {
            setNotice(t('nearest.unavailable'))
            return
        }
        setLocating(true)
        setNotice(t('nearest.locating'))
        navigator.geolocation.getCurrentPosition(
            (position) => {
                const origin = { latitude: position.coords.latitude, longitude: position.coords.longitude }
                moveMap(origin.latitude, origin.longitude, 15)
                loadNearest(origin)
            },
            (error) => {
                setLocating(false)
                setNotice(error.code === error.PERMISSION_DENIED ? t('nearest.denied') : t('nearest.unavailable'))
            },
            { enableHighAccuracy: true, timeout: 10_000, maximumAge: 60_000 },
        )
    }

    const search = (query: string) => {
        setSearching(true)
        setSearchError(null)
        searchAeds(query)
            .then((result) => {
                const found = result.value
                if (!result.isSuccess || !found || found.centerLatitude === null || found.centerLongitude === null) {
                    setSearchError(t('search.noResults', { query }))
                    return
                }
                setNearest(null)
                // One place: close up; several in a town: the town.
                moveMap(found.centerLatitude, found.centerLongitude, found.items.length === 1 ? 16 : 12)
                if (found.items.length === 1) {
                    setSelectedId(found.items[0].id)
                }
            })
            .catch(() => setSearchError(t('list.error')))
            .finally(() => setSearching(false))
    }

    const selectFromList = (point: AedMapPoint) => {
        setSelectedId(point.id)
        moveMap(point.latitude, point.longitude, 17)
    }

    const listItems: AedListItem[] = useMemo(
        () =>
            nearest
                ? nearest.items
                : [...points]
                      .sort((a, b) => a.city.localeCompare(b.city) || a.locationName.localeCompare(b.locationName))
                      .map((point) => ({ point })),
        [nearest, points],
    )

    const countText = nearest ? t('nearest.heading') : t('list.count', { count: points.length })

    return (
        <main id="main" className="flex min-h-0 flex-1 flex-col bg-white dark:bg-gray-900">
            <h1 className="sr-only">{t('title')}</h1>

            <p className="flex items-center justify-center gap-3 bg-[#c8102e] px-3 py-1.5 text-center text-sm font-semibold text-white">
                <span>{tCommon('emergency.bar')}</span>
                <a href="tel:112" className="shrink-0 rounded bg-white px-2 py-0.5 font-bold text-[#c8102e] sm:hidden">
                    112
                </a>
            </p>

            <div className="flex flex-col gap-3 border-b border-gray-200 p-3 dark:border-gray-800">
                <div className="flex flex-col gap-2 md:flex-row md:items-start">
                    <SearchBox busy={searching} error={searchError} onSearch={search} />
                    <div className="flex gap-2">
                        <Button
                            variant="solid"
                            onClick={findNearest}
                            loading={locating}
                            icon={<HiOutlineLocationMarker aria-hidden />}
                            className="flex-1 md:flex-none"
                        >
                            {t('nearest.button')}
                        </Button>
                        <Button
                            variant="default"
                            onClick={() => setFiltersOpen((open) => !open)}
                            aria-expanded={filtersOpen}
                            aria-controls="aed-filters"
                            icon={<HiOutlineFilter aria-hidden />}
                        >
                            {t('filters.heading')}
                        </Button>
                    </div>
                </div>
                <div id="aed-filters" hidden={!filtersOpen}>
                    <AedFiltersPanel filters={filters} onChange={setFilters} />
                </div>
            </div>

            {/* Phones: one of the two panes at a time. */}
            <div role="group" aria-label={t('view.label')} className="flex border-b border-gray-200 md:hidden dark:border-gray-800">
                {(['map', 'list'] as const).map((mode) => (
                    <button
                        key={mode}
                        type="button"
                        aria-pressed={view === mode}
                        onClick={() => setView(mode)}
                        className="flex-1 py-2 text-sm font-semibold text-gray-700 aria-pressed:border-b-2 aria-pressed:border-primary aria-pressed:text-primary dark:text-gray-200"
                    >
                        {t(`view.${mode}`)}
                    </button>
                ))}
            </div>

            <div className="flex min-h-0 flex-1">
                <section
                    aria-labelledby="aed-results-heading"
                    className={`${view === 'list' ? 'flex' : 'hidden'} min-h-0 w-full flex-col border-gray-200 md:flex md:w-96 md:border-r dark:border-gray-800`}
                >
                    <div className="flex items-center justify-between gap-2 border-b border-gray-200 px-4 py-2 dark:border-gray-800">
                        <h2 id="aed-results-heading" className="text-sm font-bold text-gray-900 dark:text-white">
                            <span aria-live="polite">{countText}</span>
                        </h2>
                        {loading && <Spinner size={16} />}
                        {nearest && (
                            <Button size="sm" variant="ghost" onClick={() => setNearest(null)}>
                                {t('nearest.clear')}
                            </Button>
                        )}
                    </div>
                    <div aria-live="polite">
                        {notice && <p className="px-4 py-2 text-sm text-gray-800 dark:text-gray-200">{notice}</p>}
                        {loadFailed && !nearest && (
                            <p role="alert" className="px-4 py-2 text-sm text-[#b3261e] dark:text-[#ff8a80]">
                                {t('list.error')}
                            </p>
                        )}
                        {truncated && !nearest && (
                            <p className="px-4 py-2 text-sm text-gray-700 dark:text-gray-300">{t('list.truncated')}</p>
                        )}
                    </div>
                    <div className="min-h-0 flex-1 overflow-y-auto">
                        {listItems.length === 0 && !loading && !nearest ? (
                            <p className="px-4 py-6 text-sm text-gray-700 dark:text-gray-300">{t('list.empty')}</p>
                        ) : (
                            <AedList items={listItems} selectedId={selectedId} onSelect={selectFromList} />
                        )}
                    </div>
                </section>

                <div className={`${view === 'map' ? 'block' : 'hidden'} relative min-h-0 flex-1 md:block`}>
                    <AedMap
                        points={nearest ? nearest.items.map((i) => i.point) : points}
                        selectedId={selectedId}
                        userPosition={nearest?.origin ?? null}
                        flyTo={flyTo}
                        onBoundsChange={setBounds}
                        onSelect={setSelectedId}
                    />
                </div>
            </div>

            <AedCard id={selectedId} userPosition={nearest?.origin ?? null} onClose={() => setSelectedId(null)} />
        </main>
    )
}

export default AedMapPage
