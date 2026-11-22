'use client'

import { useId, useState, type FormEvent } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import Button from '@/components/ui/Button'
import { INPUT_CLASS } from '@/components/panel/styles'
import { INPUT_LIMITS } from '@/constants/input-limits.constant'
import { sendReport } from '@/services/aed/client'

/**
 * "Report a correction" on the AED card: a short form opened in place (inside the card drawer, so the drawer
 * does not take the clicks as outside ones). Errors under the fields; a hidden honeypot field stops simple bots.
 */
const ReportCorrection = ({ aedId }: { aedId: string }) => {
    const t = useTranslations('aedMap')
    const locale = useLocale()
    const messageId = useId()
    const contactId = useId()
    const [open, setOpen] = useState(false)
    const [message, setMessage] = useState('')
    const [contact, setContact] = useState('')
    const [website, setWebsite] = useState('')
    const [errors, setErrors] = useState<Record<string, string>>({})
    const [status, setStatus] = useState<'idle' | 'sending' | 'sent' | 'failed'>('idle')

    const submit = async (event: FormEvent) => {
        event.preventDefault()
        if (message.trim().length < 10) {
            setErrors({ message: t('reportDialog.tooShort') })
            return
        }
        setErrors({})
        setStatus('sending')
        const result = await sendReport(aedId, { message, reporterContact: contact, website }, locale)
        if (result.isSuccess) {
            setStatus('sent')
            return
        }
        setErrors(result.error?.fields ?? {})
        setStatus(result.error?.fields ? 'idle' : 'failed')
    }

    if (status === 'sent') {
        return (
            <p role="status" className="rounded-lg border border-[#00843a] bg-primary-subtle p-3 text-sm text-gray-900 dark:text-gray-100">
                {t('reportDialog.sent')}
            </p>
        )
    }

    if (!open) {
        return (
            <Button variant="default" block onClick={() => setOpen(true)} aria-expanded={false}>
                {t('card.report')}
            </Button>
        )
    }

    return (
        <form onSubmit={submit} noValidate className="flex flex-col gap-3 rounded-lg border border-gray-200 p-3 dark:border-gray-800" aria-label={t('reportDialog.title')}>
            <h3 className="text-base font-bold text-gray-900 dark:text-white">{t('reportDialog.title')}</h3>
            <p className="text-sm text-gray-700 dark:text-gray-300">{t('reportDialog.intro')}</p>
            <div className="flex flex-col gap-1">
                <label htmlFor={messageId} className="text-sm font-semibold">
                    {t('reportDialog.message')} <span aria-hidden className="text-[#b3261e]">*</span>
                </label>
                <textarea
                    id={messageId}
                    value={message}
                    onChange={(e) => setMessage(e.target.value)}
                    rows={4}
                    required
                    maxLength={INPUT_LIMITS.reportMessage}
                    aria-invalid={!!errors.message}
                    aria-describedby={errors.message ? `${messageId}-error` : undefined}
                    className={INPUT_CLASS}
                />
                {errors.message && (
                    <p id={`${messageId}-error`} className="text-sm text-[#b3261e] dark:text-[#ff8a80]">
                        {errors.message}
                    </p>
                )}
            </div>
            <div className="flex flex-col gap-1">
                <label htmlFor={contactId} className="text-sm font-semibold">
                    {t('reportDialog.contact')}
                </label>
                <input
                    id={contactId}
                    value={contact}
                    onChange={(e) => setContact(e.target.value)}
                    maxLength={INPUT_LIMITS.reporterContact}
                    autoComplete="email"
                    aria-invalid={!!errors.reporterContact}
                    aria-describedby={`${contactId}-hint`}
                    className={INPUT_CLASS}
                />
                <p id={`${contactId}-hint`} className="text-xs text-gray-600 dark:text-gray-400">
                    {t('reportDialog.contactHint')}
                </p>
                {errors.reporterContact && <p className="text-sm text-[#b3261e] dark:text-[#ff8a80]">{errors.reporterContact}</p>}
            </div>
            {/* Honeypot: hidden from people and screen readers. */}
            <input
                type="text"
                name="website"
                value={website}
                onChange={(e) => setWebsite(e.target.value)}
                tabIndex={-1}
                autoComplete="off"
                aria-hidden
                className="absolute -left-[9999px] h-0 w-0 opacity-0"
            />
            {status === 'failed' && (
                <p role="alert" className="text-sm text-[#b3261e] dark:text-[#ff8a80]">
                    {t('reportDialog.error')}
                </p>
            )}
            <div className="flex gap-2">
                <Button type="submit" variant="solid" loading={status === 'sending'}>
                    {t('reportDialog.send')}
                </Button>
                <Button type="button" variant="default" onClick={() => setOpen(false)}>
                    {t('reportDialog.close')}
                </Button>
            </div>
        </form>
    )
}

export default ReportCorrection
