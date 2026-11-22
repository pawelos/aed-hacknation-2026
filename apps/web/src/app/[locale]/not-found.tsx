import { getTranslations } from 'next-intl/server'
import InfoPage from '@/components/template/InfoPage'

/** Localized 404 inside the app layout (header, language, theme), for notFound() and unknown addresses. */
export default async function NotFound() {
    const t = await getTranslations('common')

    return (
        <InfoPage code="404" title={t('notFound.title')} links={[{ href: '/', label: t('notFound.map'), primary: true }]}>
            <p>{t('notFound.body')}</p>
        </InfoPage>
    )
}
