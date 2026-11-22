'use client'

import type { ReactNode } from 'react'
import { HiOutlineTrash } from 'react-icons/hi'
import Button from '@/components/ui/Button'

/** What the form does after a successful save: stay open or go back to the list. */
export type SaveAfter = 'stay' | 'return'

type Props = {
    /** Id of the form the save buttons submit (they live in the drawer footer, outside the form). */
    formId: string
    /** Editing an existing record (update) or creating a new one. */
    editing: boolean
    pending: boolean
    labels: {
        create: string
        createAndReturn: string
        update: string
        updateAndReturn: string
        cancel: string
        delete: string
    }
    onCancel: () => void
    /** Delete is offered while editing; without it cancel is shown. */
    onDelete?: () => void
    /** Extra actions next to delete / cancel. */
    extra?: ReactNode
}

/**
 * Footer of a record drawer (pattern of Car Reminder): delete (or cancel) on the left, save buttons on the right.
 * "Create / Update" saves and stays in the form, "... and return" saves and closes it. The save buttons submit
 * the form (`form` attribute, `value` = what to do after), so Enter in a field saves as "stay".
 */
const DrawerFormFooter = ({
    formId,
    editing,
    pending,
    labels,
    onCancel,
    onDelete,
    extra,
}: Props) => (
    <div className="flex w-full flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap gap-2">
            {editing && onDelete ? (
                <Button
                    type="button"
                    variant="danger"
                    icon={<HiOutlineTrash aria-hidden />}
                    disabled={pending}
                    onClick={onDelete}
                >
                    {labels.delete}
                </Button>
            ) : (
                <Button type="button" onClick={onCancel} disabled={pending}>
                    {labels.cancel}
                </Button>
            )}
            {extra}
        </div>
        <div className="flex flex-wrap justify-end gap-2">
            <Button
                type="submit"
                form={formId}
                name="after"
                value="stay"
                variant="default"
                loading={pending}
            >
                {editing ? labels.update : labels.create}
            </Button>
            <Button
                type="submit"
                form={formId}
                name="after"
                value="return"
                variant="solid"
                loading={pending}
            >
                {editing ? labels.updateAndReturn : labels.createAndReturn}
            </Button>
        </div>
    </div>
)

export default DrawerFormFooter
