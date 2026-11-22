'use client'

import { useId, type ReactNode } from 'react'
import Button from '@/components/ui/Button'
import Dialog from '@/components/ui/Dialog'

type Props = {
    isOpen: boolean
    title: string
    children: ReactNode
    confirmText: string
    cancelText: string
    closeLabel: string
    /** Red confirm button (delete, discard). */
    danger?: boolean
    pending?: boolean
    onConfirm: () => void
    onCancel: () => void
}

/**
 * Confirmation window instead of window.confirm: named by its heading, focus trapped inside, Esc and the overlay
 * cancel. Render it inside the drawer it belongs to, so its clicks do not count as clicks outside the drawer.
 */
const ConfirmDialog = ({
    isOpen,
    title,
    children,
    confirmText,
    cancelText,
    closeLabel,
    danger,
    pending,
    onConfirm,
    onCancel,
}: Props) => {
    const titleId = useId()
    return (
        <Dialog
            isOpen={isOpen}
            onClose={onCancel}
            width={440}
            labelledBy={titleId}
            closeLabel={closeLabel}
        >
            <h2
                id={titleId}
                className="pr-10 text-lg font-bold text-gray-900 dark:text-white"
            >
                {title}
            </h2>
            <div className="mt-2 text-sm text-gray-700 dark:text-gray-300">
                {children}
            </div>
            <div className="mt-6 flex flex-wrap justify-end gap-2">
                <Button type="button" onClick={onCancel} disabled={pending}>
                    {cancelText}
                </Button>
                <Button
                    type="button"
                    variant={danger ? 'danger' : 'solid'}
                    onClick={onConfirm}
                    loading={pending}
                >
                    {confirmText}
                </Button>
            </div>
        </Dialog>
    )
}

export default ConfirmDialog
