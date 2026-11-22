import type { ReactNode } from 'react'

/**
 * Title of a panel page as in Car Reminder ("Pojazdy" + a gray line under it), with optional actions on the right
 * (they go under the title on phones).
 */
const PageHeader = ({
    title,
    description,
    actions,
}: {
    title: string
    description?: ReactNode
    actions?: ReactNode
}) => (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div className="min-w-0">
            <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl dark:text-white">
                {title}
            </h1>
            {description && (
                <p className="mt-1 max-w-3xl text-sm text-gray-700 sm:text-base dark:text-gray-300">
                    {description}
                </p>
            )}
        </div>
        {actions && <div className="flex flex-wrap gap-2">{actions}</div>}
    </div>
)

export default PageHeader
