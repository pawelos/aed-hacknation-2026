import type { ReactNode } from 'react'
import { Link } from '@/i18n/navigation'

/** One link of an information page (locale-aware). */
export type InfoLink = { href: string; label: string; primary?: boolean }

/**
 * Simple centred page with a title, text and links: the holder panel placeholder and the 404 page.
 */
const InfoPage = ({
    code,
    title,
    children,
    links,
}: {
    code?: string
    title: string
    children: ReactNode
    links: InfoLink[]
}) => (
    <main id="main" className="flex min-h-0 flex-1 overflow-y-auto bg-white dark:bg-gray-900">
        <div className="m-auto flex max-w-xl flex-col items-center gap-4 px-4 py-12 text-center text-gray-800 dark:text-gray-200">
            {code && (
                <p aria-hidden className="text-5xl font-bold text-primary dark:text-[#5bd98c]">
                    {code}
                </p>
            )}
            <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl dark:text-white">{title}</h1>
            <div className="text-base leading-relaxed">{children}</div>
            <ul className="mt-2 flex flex-col gap-2 sm:flex-row">
                {links.map((link) => (
                    <li key={link.href}>
                        <Link
                            href={link.href}
                            className={
                                'inline-flex rounded-lg px-4 py-2 text-sm font-semibold focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary ' +
                                (link.primary
                                    ? 'bg-primary text-white hover:bg-primary-deep'
                                    : 'border border-gray-300 text-gray-900 hover:bg-gray-100 dark:border-gray-700 dark:text-gray-100 dark:hover:bg-gray-800')
                            }
                        >
                            {link.label}
                        </Link>
                    </li>
                ))}
            </ul>
        </div>
    </main>
)

export default InfoPage
