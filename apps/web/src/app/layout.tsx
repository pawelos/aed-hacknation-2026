import type { ReactNode } from 'react'

// The document (<html>, providers) is rendered by [locale]/layout.tsx; every page address starts with its language.
export default function RootLayout({ children }: { children: ReactNode }) {
    return children
}
