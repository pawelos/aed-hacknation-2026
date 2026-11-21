'use client'

import { useLocale, useTranslations } from 'next-intl'
import type { AedMapPoint } from '@/services/aed/types'
import AedMarkerIcon from './AedMarkerIcon'
import OpenState from './OpenState'
import { formatDistance, isDimmed } from './aedFormat'

export type AedListItem = { point: AedMapPoint; distanceMeters?: number }

type AedListProps = {
    items: AedListItem[]
    selectedId: string | null
    onSelect: (point: AedMapPoint) => void
}

/** Most rows rendered at once; the map shows the rest, the count tells how many there are. */
const MAX_ROWS = 200

/**
 * The results as a list: works without the map (keyboard, screen readers). Each row opens the AED card
 * and moves the map to the device.
 */
const AedList = ({ items, selectedId, onSelect }: AedListProps) => {
    const t = useTranslations('aedMap')
    const tEnums = useTranslations('enums')
    const locale = useLocale()

    return (
        <ul className="divide-y divide-gray-200 dark:divide-gray-800">
            {items.slice(0, MAX_ROWS).map(({ point, distanceMeters }) => (
                <li key={point.id}>
                    <button
                        type="button"
                        onClick={() => onSelect(point)}
                        aria-current={point.id === selectedId ? 'true' : undefined}
                        className="flex w-full items-start gap-3 px-4 py-3 text-left hover:bg-gray-100 focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-primary aria-[current=true]:bg-primary-subtle dark:hover:bg-gray-800"
                    >
                        <span className="mt-1">
                            <AedMarkerIcon availability={point.availability} dimmed={isDimmed(point)} />
                        </span>
                        <span className="min-w-0 flex-1">
                            <span className="block font-semibold text-gray-900 dark:text-white">
                                {point.locationName}
                            </span>
                            <span className="block text-gray-700 dark:text-gray-300">
                                {point.address}, {point.city}
                            </span>
                            <span className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs">
                                <span className="text-gray-700 dark:text-gray-300">
                                    {tEnums(`availability.${point.availability}`)}
                                </span>
                                <OpenState isOpenNow={point.isOpenNow} />
                                {point.status !== 'Operational' && (
                                    <span className="font-semibold text-[#9a5b00] dark:text-[#ffc56b]">
                                        {tEnums(`status.${point.status}`)}
                                    </span>
                                )}
                            </span>
                        </span>
                        {distanceMeters !== undefined && (
                            <span className="shrink-0 text-sm font-bold text-gray-900 dark:text-white">
                                <span className="sr-only">
                                    {t('list.distance', { distance: formatDistance(distanceMeters, locale) })}
                                </span>
                                <span aria-hidden>{formatDistance(distanceMeters, locale)}</span>
                            </span>
                        )}
                    </button>
                </li>
            ))}
        </ul>
    )
}

export default AedList
