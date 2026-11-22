import { redirect } from '@/i18n/navigation'

/** The new record form is a drawer on the list now; old links open it there. */
export default async function NewAedPage({
    params,
}: {
    params: Promise<{ locale: string }>
}) {
    const { locale } = await params
    redirect({ href: { pathname: '/panel/aeds', query: { new: '1' } }, locale })
}
