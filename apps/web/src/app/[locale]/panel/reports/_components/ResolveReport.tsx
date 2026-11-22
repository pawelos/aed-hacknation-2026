'use client'

import { useState, useTransition } from 'react'
import { useTranslations } from 'next-intl'
import { HiOutlineCheck, HiOutlineX } from 'react-icons/hi'
import { useRouter } from '@/i18n/navigation'
import Button from '@/components/ui/Button'
import FormField from '@/components/panel/FormField'
import { INPUT_CLASS, NOTICE_ERROR_CLASS } from '@/components/panel/styles'
import { INPUT_LIMITS } from '@/constants/input-limits.constant'
import { resolveReportAction } from '@/server/actions/panel'

/**
 * Accept or reject a report, with an optional note. The API error of the note shows under it (linked with
 * aria-describedby), other errors above the buttons.
 */
const ResolveReport = ({ id }: { id: string }) => {
    const t = useTranslations('panel')
    const router = useRouter()
    const [note, setNote] = useState('')
    const [noteError, setNoteError] = useState<string | undefined>()
    const [message, setMessage] = useState<string | null>(null)
    const [pending, startTransition] = useTransition()

    const resolve = (accept: boolean) =>
        startTransition(async () => {
            setMessage(null)
            const result = await resolveReportAction(id, accept, note)
            if (result.ok) {
                router.refresh()
                return
            }
            setNoteError(result.fieldErrors?.resolutionNote)
            setMessage(
                result.fieldErrors?.resolutionNote
                    ? null
                    : (result.message ?? t('form.errorTitle')),
            )
        })

    return (
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start">
            <FormField
                label={t('reports.note')}
                error={noteError}
                className="flex-1"
            >
                {(props) => (
                    <input
                        {...props}
                        value={note}
                        onChange={(e) => {
                            setNote(e.target.value)
                            setNoteError(undefined)
                        }}
                        maxLength={INPUT_LIMITS.resolutionNote}
                        autoComplete="off"
                        className={INPUT_CLASS}
                    />
                )}
            </FormField>
            <div className="flex flex-col gap-2 sm:pt-6">
                <div className="flex flex-wrap gap-2">
                    <Button
                        type="button"
                        variant="default"
                        icon={<HiOutlineX aria-hidden />}
                        onClick={() => resolve(false)}
                        disabled={pending}
                        className="min-h-11"
                    >
                        {t('reports.reject')}
                    </Button>
                    <Button
                        type="button"
                        variant="solid"
                        icon={<HiOutlineCheck aria-hidden />}
                        onClick={() => resolve(true)}
                        loading={pending}
                        className="min-h-11"
                    >
                        {t('reports.accept')}
                    </Button>
                </div>
                {message && (
                    <p role="alert" className={NOTICE_ERROR_CLASS}>
                        {message}
                    </p>
                )}
            </div>
        </div>
    )
}

export default ResolveReport
