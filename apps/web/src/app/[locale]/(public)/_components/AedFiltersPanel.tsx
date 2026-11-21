'use client'

import { useId } from 'react'
import { useTranslations } from 'next-intl'
import Checkbox from '@/components/ui/Checkbox'
import {
    AED_AVAILABILITIES,
    FACILITY_TYPES,
    type AedFilters,
    type FacilityType,
} from '@/services/aed/types'
import AedMarkerIcon from './AedMarkerIcon'

type AedFiltersPanelProps = {
    filters: AedFilters
    onChange: (filters: AedFilters) => void
}

/** Availability (with the pin shape as the legend), kind of place and "available now". */
const AedFiltersPanel = ({ filters, onChange }: AedFiltersPanelProps) => {
    const t = useTranslations('aedMap')
    const tEnums = useTranslations('enums')
    const facilityId = useId()

    return (
        <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end sm:gap-6">
            <fieldset className="flex flex-col gap-1">
                <legend className="mb-1 text-xs font-semibold uppercase tracking-wide text-gray-600 dark:text-gray-400">
                    {t('filters.availability')}
                </legend>
                <div className="flex flex-wrap gap-x-4 gap-y-2">
                    {AED_AVAILABILITIES.map((availability) => (
                        <Checkbox
                            key={availability}
                            checked={filters.availability.includes(availability)}
                            onChange={(checked) =>
                                onChange({
                                    ...filters,
                                    availability: checked
                                        ? [...filters.availability, availability]
                                        : filters.availability.filter((a) => a !== availability),
                                })
                            }
                        >
                            <span className="inline-flex items-center gap-2 text-gray-800 dark:text-gray-200">
                                <AedMarkerIcon availability={availability} />
                                {tEnums(`availability.${availability}`)}
                            </span>
                        </Checkbox>
                    ))}
                </div>
            </fieldset>

            <div className="flex flex-col gap-1">
                <label
                    htmlFor={facilityId}
                    className="text-xs font-semibold uppercase tracking-wide text-gray-600 dark:text-gray-400"
                >
                    {t('filters.facilityType')}
                </label>
                <select
                    id={facilityId}
                    value={filters.facilityType}
                    onChange={(e) => onChange({ ...filters, facilityType: e.target.value as FacilityType | '' })}
                    className="h-10 rounded-lg border border-gray-300 bg-white px-3 text-gray-900 focus-visible:outline-2 focus-visible:outline-primary dark:border-gray-700 dark:bg-gray-900 dark:text-gray-100"
                >
                    <option value="">{t('filters.anyFacility')}</option>
                    {FACILITY_TYPES.map((type) => (
                        <option key={type} value={type}>
                            {tEnums(`facilityType.${type}`)}
                        </option>
                    ))}
                </select>
            </div>

            <Checkbox checked={filters.openNow} onChange={(checked) => onChange({ ...filters, openNow: checked })}>
                <span className="font-semibold text-gray-800 dark:text-gray-200">{t('filters.openNow')}</span>
            </Checkbox>
        </div>
    )
}

export default AedFiltersPanel
