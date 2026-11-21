import { createNavigation } from 'next-intl/navigation'
import { routing } from '@/i18n/routing'

/** Locale-aware `Link`, `redirect`, `useRouter` and `usePathname` (paths without the language prefix). */
export const { Link, redirect, usePathname, useRouter, getPathname } =
    createNavigation(routing)
