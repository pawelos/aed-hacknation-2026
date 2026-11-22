'use client'

import { useId, type ReactNode } from 'react'

/**
 * Label, control and the error under it (form or API error.fields), with the hint linked by aria-describedby.
 * The control is rendered by the caller with the given id and aria attributes.
 */
const FormField = ({
    label,
    error,
    hint,
    required,
    className,
    children,
}: {
    label: string
    error?: string
    hint?: ReactNode
    required?: boolean
    className?: string
    children: (props: {
        id: string
        'aria-invalid': boolean
        'aria-describedby'?: string
        required?: boolean
    }) => ReactNode
}) => {
    const id = useId()
    const hintId = `${id}-hint`
    const errorId = `${id}-error`
    const describedBy =
        [hint ? hintId : null, error ? errorId : null]
            .filter(Boolean)
            .join(' ') || undefined

    return (
        <div className={'flex flex-col gap-1 ' + (className ?? '')}>
            <label
                htmlFor={id}
                className="text-sm font-semibold text-gray-800 dark:text-gray-200"
            >
                {label}
                {required && (
                    <span
                        aria-hidden
                        className="text-[#b3261e] dark:text-[#ff8a80]"
                    >
                        {' '}
                        *
                    </span>
                )}
            </label>
            {children({
                id,
                'aria-invalid': !!error,
                'aria-describedby': describedBy,
                required,
            })}
            {hint && (
                <p
                    id={hintId}
                    className="text-xs text-gray-600 dark:text-gray-400"
                >
                    {hint}
                </p>
            )}
            {error && (
                <p
                    id={errorId}
                    className="text-sm text-[#b3261e] dark:text-[#ff8a80]"
                >
                    {error}
                </p>
            )}
        </div>
    )
}

export default FormField

/** Line above a form that explains the asterisk of the required fields (WCAG 3.3.2). */
export const RequiredLegend = ({ children }: { children: ReactNode }) => (
    <p className="text-sm text-gray-700 dark:text-gray-300">
        <span aria-hidden className="text-[#b3261e] dark:text-[#ff8a80]">
            *{' '}
        </span>
        {children}
    </p>
)
