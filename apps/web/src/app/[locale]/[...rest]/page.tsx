import { notFound } from 'next/navigation'

// Unknown address within a language: the localized [locale]/not-found.tsx (as in Car Reminder).
export default function CatchAllPage() {
    notFound()
}
