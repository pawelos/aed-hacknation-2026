import { Suspense } from 'react'
import { setRequestLocale } from 'next-intl/server'
import AedMapPage from './_components/AedMapPage'

/** Home page: the public map of AED devices. */
export default async function HomePage({ params }: { params: Promise<{ locale: string }> }) {
    const { locale } = await params
    setRequestLocale(locale)

    return (
        // The filters are read from the address (useSearchParams), which needs a Suspense boundary.
        <Suspense>
            <AedMapPage />
        </Suspense>
    )
}
