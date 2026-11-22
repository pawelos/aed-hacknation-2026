'use client'

import {
    useEffect,
    useRef,
    useState,
    useTransition,
    type FormEvent,
    type ReactNode,
} from 'react'
import { useLocale, useTranslations } from 'next-intl'
import FormField, { RequiredLegend } from '@/components/panel/FormField'
import LookupSelect from '@/components/panel/LookupSelect'
import type { SaveAfter } from '@/components/panel/DrawerFormFooter'
import {
    GROUP_LABEL_CLASS,
    INPUT_CLASS,
    NOTICE_ERROR_CLASS,
} from '@/components/panel/styles'
import { INPUT_LIMITS } from '@/constants/input-limits.constant'
import { saveAedAction } from '@/server/actions/panel'
import {
    AED_AVAILABILITIES,
    AED_STATUSES,
    FACILITY_TYPES,
} from '@/services/aed/types'
import {
    VOIVODESHIPS,
    type AedInput,
    type DataSource,
} from '@/services/panel/types'
import { describeOpeningHours } from '@/utils/openingHours'

type Props = {
    /** Id of the form element; the save buttons in the drawer footer submit it. */
    formId: string
    id?: string
    initial: AedInput
    sources?: DataSource[]
    onPendingChange: (pending: boolean) => void
    onDirtyChange: (dirty: boolean) => void
    /** After a successful save, with the record id (new for a created record). */
    onSaved: (after: SaveAfter, id: string) => void
}

type TextField = Exclude<
    keyof AedInput,
    'isIndoor' | 'facilityType' | 'availability' | 'status' | 'voivodeship'
>

/** Group of fields with a small upper-case heading, as in Car Reminder (outside the form, so typing does not remount the inputs). */
const Section = ({
    title,
    children,
}: {
    title: string
    children: ReactNode
}) => (
    <fieldset className="grid min-w-0 gap-x-4 gap-y-5 sm:grid-cols-2">
        <legend className={`${GROUP_LABEL_CLASS} mb-3`}>{title}</legend>
        {children}
    </fieldset>
)

/**
 * Form of an AED record (new or edit) in the record drawer. Errors of the form and of the API (error.fields,
 * camelCase as the field names) show under their fields, and the first of them gets the focus; maxLength equals
 * the API limits (INPUT_LIMITS). The save buttons live in the drawer footer (DrawerFormFooter).
 */
const AedForm = ({
    formId,
    id,
    initial,
    sources,
    onPendingChange,
    onDirtyChange,
    onSaved,
}: Props) => {
    const t = useTranslations('panel')
    const tEnums = useTranslations('enums')
    const locale = useLocale()
    const formRef = useRef<HTMLFormElement>(null)
    const initialSource = sources?.length === 1 ? sources[0].id : ''
    const [values, setValues] = useState<AedInput>(initial)
    const [dataSourceId, setDataSourceId] = useState(initialSource)
    const [baseline, setBaseline] = useState(() =>
        JSON.stringify([initial, initialSource]),
    )
    const [errors, setErrors] = useState<Record<string, string>>({})
    const [errorTick, setErrorTick] = useState(0)
    const [message, setMessage] = useState<string | null>(null)
    const [pending, startTransition] = useTransition()

    const dirty = JSON.stringify([values, dataSourceId]) !== baseline

    useEffect(() => onPendingChange(pending), [pending, onPendingChange])
    useEffect(() => onDirtyChange(dirty), [dirty, onDirtyChange])

    // After a failed save the first field with an error gets the focus (and scrolls into view).
    useEffect(() => {
        if (errorTick > 0) {
            formRef.current
                ?.querySelector<HTMLElement>('[aria-invalid="true"]')
                ?.focus()
        }
    }, [errorTick])

    const set = <K extends keyof AedInput>(key: K, value: AedInput[K]) => {
        setValues((v) => ({ ...v, [key]: value }))
        // The user is fixing this field: drop its message.
        setErrors((prev) => {
            if (!(key in prev)) return prev
            const next = { ...prev }
            delete next[key]
            return next
        })
    }

    const validate = (): Record<string, string> => {
        const found: Record<string, string> = {}
        const required: (keyof AedInput)[] = [
            'locationName',
            'address',
            'city',
            'voivodeship',
            'latitude',
            'longitude',
        ]
        required.forEach((key) => {
            if (String(values[key]).trim() === '')
                found[key] = t('form.required')
        })
        if (!id && !dataSourceId) found.dataSourceId = t('form.required')
        if (
            values.postalCode &&
            !/^\d{2}-\d{3}$/.test(values.postalCode.trim())
        )
            found.postalCode = t('form.invalidPostalCode')
        if (
            values.municipalityTeryt &&
            !/^\d{7}$/.test(values.municipalityTeryt.trim())
        )
            found.municipalityTeryt = t('form.invalidTeryt')
        ;(['latitude', 'longitude'] as const).forEach((key) => {
            if (
                values[key].trim() &&
                Number.isNaN(Number(values[key].replace(',', '.')))
            )
                found[key] = t('form.invalidCoordinate')
        })
        return found
    }

    const showErrors = (found: Record<string, string>) => {
        setErrors(found)
        setErrorTick((n) => n + 1)
    }

    const submit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault()
        if (pending) {
            return
        }
        const submitter = (event.nativeEvent as SubmitEvent)
            .submitter as HTMLButtonElement | null
        const after: SaveAfter =
            submitter?.value === 'return' ? 'return' : 'stay'
        const found = validate()
        setMessage(null)
        if (Object.keys(found).length > 0) {
            showErrors(found)
            return
        }
        setErrors({})
        startTransition(async () => {
            const result = await saveAedAction({
                ...values,
                id,
                dataSourceId: id ? undefined : dataSourceId,
                voivodeship: values.voivodeship || null,
                latitude: Number(values.latitude.replace(',', '.')),
                longitude: Number(values.longitude.replace(',', '.')),
            })
            if (result.ok) {
                setBaseline(JSON.stringify([values, dataSourceId]))
                onSaved(after, result.id ?? id ?? '')
                return
            }
            // API field errors under their fields first, the rest as a message.
            if (result.fieldErrors) {
                showErrors(result.fieldErrors)
            }
            setMessage(
                result.message ??
                    (result.fieldErrors
                        ? t('form.fixErrors')
                        : t('form.errorTitle')),
            )
        })
    }

    const text = (
        key: TextField,
        options: {
            required?: boolean
            hint?: ReactNode
            maxLength: number
            className?: string
            inputMode?: 'decimal' | 'tel' | 'numeric'
        },
    ) => (
        <FormField
            label={t(`form.${key}`)}
            error={errors[key]}
            hint={options.hint}
            required={options.required}
            className={options.className}
        >
            {(props) => (
                <input
                    {...props}
                    name={key}
                    value={values[key]}
                    onChange={(e) => set(key, e.target.value)}
                    maxLength={options.maxLength}
                    inputMode={options.inputMode}
                    // Data of the device, not of the user (WCAG 1.3.5 does not apply): no browser autofill.
                    autoComplete="off"
                    className={INPUT_CLASS}
                />
            )}
        </FormField>
    )

    // The opening_hours notation uses English abbreviations: the hint explains them and shows the value read in the page language.
    const hoursText = describeOpeningHours(values.availabilityHours, locale, {
        always: t('form.hoursAlways'),
        holidays: t('form.hoursHolidays'),
        closed: t('form.hoursClosed'),
    })
    const hoursHint = (
        <>
            {t('form.hoursHint')}
            {hoursText && (
                <span className="mt-1 block font-semibold text-gray-800 dark:text-gray-200">
                    {t('form.hoursPreview', { text: hoursText })}
                </span>
            )}
        </>
    )

    const lat = Number(values.latitude.replace(',', '.'))
    const lng = Number(values.longitude.replace(',', '.'))

    return (
        <form
            id={formId}
            ref={formRef}
            onSubmit={submit}
            noValidate
            className="flex flex-col gap-8"
        >
            <RequiredLegend>{t('form.requiredLegend')}</RequiredLegend>
            {message && (
                <p role="alert" className={NOTICE_ERROR_CLASS}>
                    {message}
                </p>
            )}
            <Section title={t('form.location')}>
                {!id && sources && (
                    <FormField
                        label={t('form.dataSource')}
                        error={errors.dataSourceId}
                        required
                        className="sm:col-span-2"
                    >
                        {(props) => (
                            <LookupSelect
                                label={t('form.dataSource')}
                                fieldProps={props}
                                value={dataSourceId}
                                options={sources.map((source) => ({
                                    value: source.id,
                                    label: source.name,
                                    extra: source.ownerName,
                                }))}
                                extraHeader={t('form.colOwner')}
                                onChange={(next) => {
                                    setDataSourceId(next)
                                    setErrors(
                                        ({ dataSourceId: _removed, ...rest }) =>
                                            rest,
                                    )
                                }}
                            />
                        )}
                    </FormField>
                )}
                {text('locationName', {
                    required: true,
                    maxLength: INPUT_LIMITS.locationName,
                    className: 'sm:col-span-2',
                })}
                {text('address', {
                    required: true,
                    maxLength: INPUT_LIMITS.address,
                })}
                {text('postalCode', { maxLength: INPUT_LIMITS.postalCode })}
                {text('city', { required: true, maxLength: INPUT_LIMITS.city })}
                {text('municipality', { maxLength: INPUT_LIMITS.municipality })}
                {text('county', { maxLength: INPUT_LIMITS.county })}
                <FormField
                    label={t('form.voivodeship')}
                    error={errors.voivodeship}
                    required
                >
                    {(props) => (
                        <LookupSelect
                            label={t('form.voivodeship')}
                            fieldProps={props}
                            value={values.voivodeship}
                            options={VOIVODESHIPS.map((v, index) => ({
                                value: v,
                                label: tEnums(`voivodeship.${v}`),
                                extra: String((index + 1) * 2).padStart(2, '0'),
                            }))}
                            extraHeader={t('form.colCode')}
                            onChange={(next) =>
                                set(
                                    'voivodeship',
                                    next as AedInput['voivodeship'],
                                )
                            }
                        />
                    )}
                </FormField>
                {text('municipalityTeryt', {
                    maxLength: INPUT_LIMITS.municipalityTeryt,
                    inputMode: 'numeric',
                })}
                {text('sourceRecordId', {
                    maxLength: INPUT_LIMITS.sourceRecordId,
                })}
                {text('latitude', {
                    required: true,
                    maxLength: INPUT_LIMITS.coordinate,
                    inputMode: 'decimal',
                })}
                {text('longitude', {
                    required: true,
                    maxLength: INPUT_LIMITS.coordinate,
                    inputMode: 'decimal',
                })}
                <p className="text-xs text-gray-600 sm:col-span-2 dark:text-gray-400">
                    {t('form.coordinatesHint')}{' '}
                    {!Number.isNaN(lat) &&
                        !Number.isNaN(lng) &&
                        values.latitude &&
                        values.longitude && (
                            <a
                                href={`https://www.openstreetmap.org/?mlat=${lat}&mlon=${lng}#map=18/${lat}/${lng}`}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-[#1a56db] underline focus-visible:outline-2 focus-visible:outline-primary dark:text-[#8ab4ff]"
                            >
                                {t('form.openMap')}
                            </a>
                        )}
                </p>
            </Section>

            <Section title={t('form.place')}>
                {text('placementDetails', {
                    maxLength: INPUT_LIMITS.placementDetails,
                    className: 'sm:col-span-2',
                })}
                <FormField
                    label={t('form.facilityType')}
                    error={errors.facilityType}
                >
                    {(props) => (
                        <LookupSelect
                            label={t('form.facilityType')}
                            fieldProps={props}
                            value={values.facilityType}
                            options={FACILITY_TYPES.map((f) => ({
                                value: f,
                                label: tEnums(`facilityType.${f}`),
                            }))}
                            onChange={(next) =>
                                set(
                                    'facilityType',
                                    next as AedInput['facilityType'],
                                )
                            }
                        />
                    )}
                </FormField>
                <label className="flex min-h-11 cursor-pointer items-center gap-3 self-end text-sm text-gray-800 dark:text-gray-200">
                    <input
                        type="checkbox"
                        checked={values.isIndoor}
                        onChange={(e) => set('isIndoor', e.target.checked)}
                        className="h-5 w-5 shrink-0 accent-[#00843a] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                    />
                    {t('form.isIndoor')}
                </label>
            </Section>

            <Section title={t('form.access')}>
                <FormField
                    label={t('form.availability')}
                    error={errors.availability}
                >
                    {(props) => (
                        <LookupSelect
                            label={t('form.availability')}
                            fieldProps={props}
                            value={values.availability}
                            options={AED_AVAILABILITIES.map((a) => ({
                                value: a,
                                label: tEnums(`availability.${a}`),
                            }))}
                            onChange={(next) =>
                                set(
                                    'availability',
                                    next as AedInput['availability'],
                                )
                            }
                        />
                    )}
                </FormField>
                {text('availabilityHours', {
                    maxLength: INPUT_LIMITS.availabilityHours,
                    hint: hoursHint,
                })}
                {text('accessNotes', {
                    maxLength: INPUT_LIMITS.accessNotes,
                    className: 'sm:col-span-2',
                })}
            </Section>

            <Section title={t('form.other')}>
                {text('contactPhone', {
                    maxLength: INPUT_LIMITS.contactPhone,
                    inputMode: 'tel',
                })}
                <FormField label={t('form.status')} error={errors.status}>
                    {(props) => (
                        <LookupSelect
                            label={t('form.status')}
                            fieldProps={props}
                            value={values.status}
                            options={AED_STATUSES.map((st) => ({
                                value: st,
                                label: tEnums(`status.${st}`),
                            }))}
                            onChange={(next) =>
                                set('status', next as AedInput['status'])
                            }
                        />
                    )}
                </FormField>
            </Section>
        </form>
    )
}

export default AedForm
