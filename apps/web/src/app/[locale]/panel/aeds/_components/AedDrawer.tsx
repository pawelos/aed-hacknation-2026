'use client'

import { useCallback, useId, useState, useTransition } from 'react'
import { useFormatter, useTranslations } from 'next-intl'
import {
    HiOutlineArrowsExpand,
    HiOutlineCheckCircle,
    HiOutlineExclamation,
} from 'react-icons/hi'
import Button from '@/components/ui/Button'
import Drawer from '@/components/ui/Drawer'
import Tooltip from '@/components/ui/Tooltip'
import ConfirmDialog from '@/components/panel/ConfirmDialog'
import {
    NOTICE_ERROR_CLASS,
    NOTICE_SUCCESS_CLASS,
} from '@/components/panel/styles'
import DrawerFormFooter, {
    type SaveAfter,
} from '@/components/panel/DrawerFormFooter'
import { deleteAedAction, verifyAedAction } from '@/server/actions/panel'
import type { AedDetails, DataSource } from '@/services/panel/types'
import AedForm from './AedForm'
import { aedToInput, EMPTY_AED } from './aedInput'

type Props = {
    open: boolean
    /** The record being edited; none for a new record. */
    record?: AedDetails
    sources: DataSource[]
    /** Close the drawer; `saved` / `deleted` shows the matching message over the list. */
    onClose: (result?: 'saved' | 'deleted') => void
    /** A new record was saved with "Create": the drawer turns into its edit view. */
    onCreated: (id: string) => void
    /** The record changed on the server (update, verification): load it again. */
    onChanged: () => void
}

type Status = { forId: string; text: string; error?: boolean }

/**
 * Record drawer of the AED list (pattern of Car Reminder's vehicles): title with full-screen toggle, the form, and
 * the footer with delete | update, update and return. Closing with unsaved changes asks first. The dialogs are
 * rendered inside the drawer, so their clicks do not count as clicks outside it.
 */
const AedDrawer = ({
    open,
    record,
    sources,
    onClose,
    onCreated,
    onChanged,
}: Props) => {
    const t = useTranslations('panel')
    const format = useFormatter()
    const titleId = useId()
    const formId = useId()
    const [expanded, setExpanded] = useState(false)
    const [formPending, setFormPending] = useState(false)
    const [dirty, setDirty] = useState(false)
    const [discardOpen, setDiscardOpen] = useState(false)
    const [deleteOpen, setDeleteOpen] = useState(false)
    const [status, setStatus] = useState<Status | null>(null)
    const [actionPending, startAction] = useTransition()

    // While the drawer slides out, it keeps showing the last record (the URL no longer names it).
    const [shown, setShown] = useState<AedDetails | undefined>(record)
    if (open && record !== shown) {
        setShown(record)
    }
    const aed = open ? record : shown
    const currentId = aed?.id ?? 'new'
    const pending = formPending || actionPending
    const visibleStatus = status?.forId === currentId ? status : null

    const handlePending = useCallback(
        (value: boolean) => setFormPending(value),
        [],
    )
    const handleDirty = useCallback((value: boolean) => setDirty(value), [])

    const close = (result?: 'saved' | 'deleted') => {
        setDiscardOpen(false)
        setDeleteOpen(false)
        setExpanded(false)
        setDirty(false)
        setStatus(null)
        onClose(result)
    }

    const requestClose = () => {
        if (pending) {
            return
        }
        if (dirty) {
            setDiscardOpen(true)
            return
        }
        close()
    }

    const handleSaved = (after: SaveAfter, id: string) => {
        if (after === 'return' || !id) {
            close('saved')
            return
        }
        setStatus({ forId: id, text: t('form.saved') })
        if (aed) {
            onChanged()
        } else {
            onCreated(id)
        }
    }

    const verify = () => {
        if (!aed) return
        startAction(async () => {
            const result = await verifyAedAction(aed.id)
            setStatus({
                forId: aed.id,
                text: result.ok
                    ? t('form.verified')
                    : (result.message ?? t('form.errorTitle')),
                error: !result.ok,
            })
            if (result.ok) onChanged()
        })
    }

    const remove = () => {
        if (!aed) return
        startAction(async () => {
            const result = await deleteAedAction(aed.id)
            setDeleteOpen(false)
            if (result.ok) {
                close('deleted')
            } else {
                setStatus({
                    forId: aed.id,
                    text: result.message ?? t('form.errorTitle'),
                    error: true,
                })
            }
        })
    }

    const title = aed
        ? t('form.titleEditNamed', { name: aed.locationName })
        : t('form.titleNew')

    return (
        <Drawer
            isOpen={open}
            onClose={requestClose}
            width={expanded ? '100vw' : 720}
            contentClassName={
                expanded ? 'm-0! rounded-none! max-w-none!' : undefined
            }
            labelledBy={titleId}
            closeLabel={t('form.close')}
            shouldCloseOnEsc={!discardOpen && !deleteOpen}
            title={
                <div className="flex items-center gap-2 pr-10">
                    <h2
                        id={titleId}
                        className="text-base font-semibold text-gray-900 sm:text-lg dark:text-white"
                    >
                        {title}
                    </h2>
                    <Tooltip
                        title={
                            expanded
                                ? t('form.exitFullScreen')
                                : t('form.fullScreen')
                        }
                    >
                        <Button
                            type="button"
                            size="sm"
                            variant="ghost"
                            icon={<HiOutlineArrowsExpand aria-hidden />}
                            aria-label={
                                expanded
                                    ? t('form.exitFullScreen')
                                    : t('form.fullScreen')
                            }
                            aria-pressed={expanded}
                            className="hidden sm:inline-flex"
                            onClick={() => setExpanded((v) => !v)}
                        />
                    </Tooltip>
                </div>
            }
            bodyClass={expanded ? 'p-4 sm:p-8' : 'p-4 sm:p-6'}
            footer={
                <DrawerFormFooter
                    formId={formId}
                    editing={!!aed}
                    pending={pending}
                    labels={{
                        create: t('form.create'),
                        createAndReturn: t('form.createAndReturn'),
                        update: t('form.update'),
                        updateAndReturn: t('form.updateAndReturn'),
                        cancel: t('form.cancel'),
                        delete: t('aeds.delete'),
                    }}
                    onCancel={requestClose}
                    onDelete={aed ? () => setDeleteOpen(true) : undefined}
                    extra={
                        aed ? (
                            <Button
                                type="button"
                                icon={<HiOutlineCheckCircle aria-hidden />}
                                onClick={verify}
                                disabled={pending}
                            >
                                {t('form.verify')}
                            </Button>
                        ) : null
                    }
                />
            }
        >
            <div
                className={
                    expanded
                        ? 'mx-auto flex max-w-4xl flex-col gap-5'
                        : 'flex flex-col gap-5'
                }
            >
                {aed && (
                    <p className="text-sm text-gray-700 dark:text-gray-300">
                        {aed.dataSourceName} ·{' '}
                        {aed.lastVerifiedAt
                            ? t('form.lastVerified', {
                                  date: format.dateTime(
                                      new Date(aed.lastVerifiedAt),
                                      { dateStyle: 'long' },
                                  ),
                              })
                            : t('form.neverVerified')}
                    </p>
                )}
                {aed?.needsVerification && (
                    <div
                        role="note"
                        className="flex gap-2 rounded-lg border border-[#f59e0b] bg-[#fff7e6] p-3 text-sm text-[#5c3a00] dark:bg-[#3a2a00] dark:text-[#ffd98a]"
                    >
                        <HiOutlineExclamation
                            aria-hidden
                            className="mt-0.5 shrink-0 text-lg"
                        />
                        <div>
                            <p className="font-semibold">
                                {t('form.reasonTitle')}
                            </p>
                            <p>{aed.verificationReason}</p>
                        </div>
                    </div>
                )}
                <p
                    role="status"
                    className={
                        visibleStatus
                            ? visibleStatus.error
                                ? NOTICE_ERROR_CLASS
                                : NOTICE_SUCCESS_CLASS
                            : 'sr-only'
                    }
                >
                    {visibleStatus?.text}
                </p>
                <AedForm
                    key={
                        aed
                            ? `${aed.id}:${aed.updatedAt ?? ''}:${aed.lastVerifiedAt ?? ''}`
                            : 'new'
                    }
                    formId={formId}
                    id={aed?.id}
                    initial={aed ? aedToInput(aed) : EMPTY_AED}
                    sources={aed ? undefined : sources}
                    onPendingChange={handlePending}
                    onDirtyChange={handleDirty}
                    onSaved={handleSaved}
                />
            </div>

            <ConfirmDialog
                isOpen={discardOpen}
                title={t('form.discardTitle')}
                confirmText={t('form.confirm')}
                cancelText={t('form.cancel')}
                closeLabel={t('form.close')}
                danger
                onConfirm={() => close()}
                onCancel={() => setDiscardOpen(false)}
            >
                <p>{t('form.discardBody')}</p>
            </ConfirmDialog>
            <ConfirmDialog
                isOpen={deleteOpen}
                title={t('form.deleteTitleOne')}
                confirmText={t('aeds.delete')}
                cancelText={t('form.cancel')}
                closeLabel={t('form.close')}
                danger
                pending={actionPending}
                onConfirm={remove}
                onCancel={() => setDeleteOpen(false)}
            >
                <p>{t('form.confirmDelete')}</p>
            </ConfirmDialog>
        </Drawer>
    )
}

export default AedDrawer
