'use client'

import type { ReactNode } from 'react'

type Props = {
    count: number
    /** "1 record selected", "3 records selected". */
    label: string
    /** Name of the toolbar for screen readers. */
    ariaLabel: string
    children: ReactNode
}

/**
 * Selection toolbar fixed at the bottom of a list (pattern of Car Reminder). The live region stays in the page
 * when nothing is selected, so screen readers announce every change of the count.
 */
const BulkActionBar = ({ count, label, ariaLabel, children }: Props) => (
    <>
        <p aria-live="polite" className="sr-only">
            {count > 0 ? label : ''}
        </p>
        {count > 0 && (
            <div
                role="toolbar"
                aria-label={ariaLabel}
                className="fixed inset-x-4 bottom-4 z-30 mx-auto flex max-w-max flex-wrap items-center justify-center gap-2 rounded-xl border border-gray-200 bg-white px-4 py-2 shadow-xl sm:gap-3 dark:border-gray-700 dark:bg-gray-800"
            >
                <span
                    aria-hidden
                    className="inline-flex h-7 min-w-7 items-center justify-center rounded-full bg-primary px-2 text-sm font-semibold text-white"
                >
                    {count}
                </span>
                <span className="text-sm text-gray-800 dark:text-gray-200">
                    {label}
                </span>
                <div className="flex flex-wrap gap-2">{children}</div>
            </div>
        )}
    </>
)

export default BulkActionBar
