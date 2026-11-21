'use client'

import classNames from 'classnames'
import { useTranslations } from 'next-intl'
import { HiCheckCircle, HiMinusCircle, HiQuestionMarkCircle } from 'react-icons/hi'

/** "Available now / not now / no data", with an icon and text (not colour alone). */
const OpenState = ({ isOpenNow, className }: { isOpenNow: boolean | null; className?: string }) => {
    const t = useTranslations('aedMap')

    if (isOpenNow === true) {
        return (
            <span className={classNames('inline-flex items-center gap-1 font-semibold text-[#00843a] dark:text-[#5bd98c]', className)}>
                <HiCheckCircle aria-hidden /> {t('open.yes')}
            </span>
        )
    }
    if (isOpenNow === false) {
        return (
            <span className={classNames('inline-flex items-center gap-1 font-semibold text-[#b3261e] dark:text-[#ff8a80]', className)}>
                <HiMinusCircle aria-hidden /> {t('open.no')}
            </span>
        )
    }
    return (
        <span className={classNames('inline-flex items-center gap-1 text-gray-600 dark:text-gray-300', className)}>
            <HiQuestionMarkCircle aria-hidden /> {t('open.unknown')}
        </span>
    )
}

export default OpenState
