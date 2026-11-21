'use client'

import { useCallback, useMemo } from 'react'
import { useSearchParams } from 'next/navigation'
import { usePathname, useRouter } from '@/i18n/navigation'
import {
    AED_AVAILABILITIES,
    FACILITY_TYPES,
    type AedAvailability,
    type AedFilters,
    type FacilityType,
} from '@/services/aed/types'

/**
 * Filters kept in the address (?availability=Public24h&facility=School&open=1), so a filtered map can be
 * shared as a link and survives a language switch.
 */
export default function useAedFilters() {
    const searchParams = useSearchParams()
    const router = useRouter()
    const pathname = usePathname()

    const filters = useMemo<AedFilters>(() => {
        const availability = searchParams
            .getAll('availability')
            .filter((v): v is AedAvailability => (AED_AVAILABILITIES as readonly string[]).includes(v))
        const facility = searchParams.get('facility') ?? ''
        return {
            availability,
            facilityType: (FACILITY_TYPES as readonly string[]).includes(facility) ? (facility as FacilityType) : '',
            openNow: searchParams.get('open') === '1',
        }
    }, [searchParams])

    const setFilters = useCallback(
        (next: AedFilters) => {
            const params = new URLSearchParams()
            next.availability.forEach((v) => params.append('availability', v))
            if (next.facilityType) {
                params.set('facility', next.facilityType)
            }
            if (next.openNow) {
                params.set('open', '1')
            }
            const query = Object.fromEntries(
                [...params.keys()].map((key) => [key, params.getAll(key)]),
            )
            router.replace({ pathname, query }, { scroll: false })
        },
        [pathname, router],
    )

    return { filters, setFilters }
}
