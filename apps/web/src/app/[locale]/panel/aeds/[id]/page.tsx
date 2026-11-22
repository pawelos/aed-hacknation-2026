import { redirect } from '@/i18n/navigation'

/** A record is edited in the drawer of the list now; old links (e.g. from reports) open it there. */
export default async function EditAedPage({
    params,
}: {
    params: Promise<{ locale: string; id: string }>
}) {
    const { locale, id } = await params
    redirect({ href: { pathname: '/panel/aeds', query: { id } }, locale })
}
