'use client'

import { useTranslations } from 'next-intl'
import SharedLookupSelect, {
    type LookupFieldProps,
    type LookupOption,
} from '@/components/shared/LookupSelect'

export type { LookupOption } from '@/components/shared/LookupSelect'

type Props = {
    /** Name of the field: title of the window and part of the button names. */
    label: string
    value: string
    options: LookupOption[]
    onChange: (value: string) => void
    /** Header of the second column. */
    extraHeader?: string
    /** Optional fields and filters: "Clear" in the window sets an empty value. */
    clearable?: boolean
    placeholder?: string
    /** id, aria-invalid, aria-describedby and required from FormField (the error under the field). */
    fieldProps: LookupFieldProps
}

/** Lookup field of the panel (Car Reminder style) with the texts of the `panel` namespace. */
const LookupSelect = (props: Props) => {
    const t = useTranslations('panel')
    return (
        <SharedLookupSelect
            {...props}
            texts={{
                open: t('form.lookupOpen', { label: props.label }),
                searchLabel: t('form.lookupSearchLabel', {
                    label: props.label,
                }),
                search: t('form.lookupSearch'),
                placeholder: t('form.select'),
                nameHeader: t('form.colName'),
                empty: t('form.lookupEmpty'),
                clear: t('form.lookupClear'),
                close: t('form.close'),
                selected: t('form.lookupSelected'),
                count: (shown, total) =>
                    t('form.lookupCount', { shown, total }),
            }}
        />
    )
}

export default LookupSelect
