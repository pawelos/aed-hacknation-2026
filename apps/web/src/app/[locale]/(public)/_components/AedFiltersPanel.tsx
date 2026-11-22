'use client'

import { useId } from 'react'
import { useTranslations } from 'next-intl'
import Checkbox from '@/components/ui/Checkbox'
import LookupSelect from '@/components/shared/LookupSelect'
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
                            checked={filters.availability.includes(
                                availability,
                            )}
                            onChange={(checked) =>
                                onChange({
                                    ...filters,
                                    availability: checked
                                        ? [
                                              ...filters.availability,
                                              availability,
                                          ]
                                        : filters.availability.filter(
                                              (a) => a !== availability,
                                          ),
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
                <div className="sm:w-72">
                    <LookupSelect
                        label={t('filters.facilityType')}
                        fieldProps={{ id: facilityId, 'aria-invalid': false }}
                        value={filters.facilityType}
                        options={FACILITY_TYPES.map((type) => ({
                            value: type,
                            label: tEnums(`facilityType.${type}`),
                        }))}
                        placeholder={t('filters.anyFacility')}
                        clearable
                        onChange={(next) =>
                            onChange({
                                ...filters,
                                facilityType: next as FacilityType | '',
                            })
                        }
                        texts={{
                            open: t('filters.lookupOpen', {
                                label: t('filters.facilityType'),
                            }),
                            searchLabel: t('filters.lookupSearchLabel', {
                                label: t('filters.facilityType'),
                            }),
                            search: t('filters.lookupSearch'),
                            placeholder: t('filters.anyFacility'),
                            nameHeader: t('filters.colName'),
                            empty: t('filters.lookupEmpty'),
                            clear: t('filters.lookupClear'),
                            close: t('filters.close'),
                            selected: t('filters.lookupSelected'),
                            count: (shown, total) =>
                                t('filters.lookupCount', { shown, total }),
                        }}
                    />
                </div>
            </div>

            <Checkbox
                checked={filters.openNow}
                onChange={(checked) =>
                    onChange({ ...filters, openNow: checked })
                }
            >
                <span className="font-semibold text-gray-800 dark:text-gray-200">
                    {t('filters.openNow')}
                </span>
            </Checkbox>
        </div>
    )
}

export default AedFiltersPanel
