'use client'

import { useSyncExternalStore } from 'react'

const QUERY = '(prefers-color-scheme: dark)'

const subscribe = (onChange: () => void) => {
    const media = window.matchMedia(QUERY)
    media.addEventListener('change', onChange)
    return () => media.removeEventListener('change', onChange)
}

const getSnapshot = () => window.matchMedia(QUERY).matches

// The server has no media query; render light there and let hydration update it.
const getServerSnapshot = () => false

/** SSR-safe `prefers-color-scheme: dark` that also follows OS changes. */
const usePrefersDarkMode = (): boolean =>
    useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot)

export default usePrefersDarkMode
