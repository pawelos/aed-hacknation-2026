'use client'

import { useEffect, useState, type ReactNode } from 'react'
import { useFormatter, useLocale, useTranslations } from 'next-intl'
import { HiExclamation, HiExternalLink, HiOutlinePhone } from 'react-icons/hi'
import Drawer from '@/components/ui/Drawer'
import Button from '@/components/ui/Button'
import Spinner from '@/components/ui/Spinner'
import { fetchCard } from '@/services/aed/client'
import type { AedCard as AedCardData } from '@/services/aed/types'
import OpenState from './OpenState'
import ReportCorrection from './ReportCorrection'
import { directionsUrl } from './aedFormat'
import { describeOpeningHours } from '@/utils/openingHours'

type AedCardProps = {
    id: string | null
    userPosition: { latitude: number; longitude: number } | null
    onClose: () => void
}

type CardState = { id: string; card: AedCardData | null; failed: boolean }

const Row = ({ label, children }: { label: string; children: ReactNode }) => (
    <div className="py-2">
        <dt className="text-xs font-semibold uppercase tracking-wide text-gray-600 dark:text-gray-400">
            {label}
        </dt>
        <dd className="mt-0.5 text-gray-900 dark:text-gray-100">{children}</dd>
    </div>
)

/**
 * Card of one AED in a side drawer: where it is, when it can be reached and how, who holds the data,
 * where the data comes from (source, licence, reference scope) and when it was last checked.
 */
const AedCard = ({ id, userPosition, onClose }: AedCardProps) => {
    const t = useTranslations('aedMap')
    const tEnums = useTranslations('enums')
    const tCommon = useTranslations('common')
    const locale = useLocale()
    const hoursLabels = {
        always: t('card.hoursAlways'),
        holidays: t('card.hoursHolidays'),
        closed: t('card.hoursClosed'),
    }
    const format = useFormatter()
    const [state, setState] = useState<CardState | null>(null)

    useEffect(() => {
        if (!id) {
            return
        }
        const controller = new AbortController()
        fetchCard(id, controller.signal)
            .then((result) => {
                setState({
                    id,
                    card: result.isSuccess ? result.value : null,
                    failed: !result.isSuccess,
                })
            })
            .catch(() => {
                if (!controller.signal.aborted) {
                    setState({ id, card: null, failed: true })
                }
            })
        return () => controller.abort()
    }, [id])

    const current = state && state.id === id ? state : null
    const card = current?.card ?? null

    return (
        <Drawer
            isOpen={id !== null}
            onClose={onClose}
            placement="right"
            width="min(440px, 100vw)"
            title={
                card?.locationName ??
                (current?.failed ? t('card.notFound') : tCommon('loading'))
            }
            bodyClass="overflow-y-auto"
        >
            {!current && (
                <div className="flex justify-center py-10" role="status">
                    <Spinner size={32} />
                    <span className="sr-only">{tCommon('loading')}</span>
                </div>
            )}
            {current?.failed && <p role="alert">{t('card.notFound')}</p>}
            {card && (
                <div className="flex flex-col gap-4 text-sm">
                    <div className="flex flex-wrap items-center gap-3">
                        <OpenState
                            isOpenNow={card.isOpenNow}
                            className="text-base"
                        />
                        <span className="text-gray-700 dark:text-gray-300">
                            {tEnums(`availability.${card.availability}`)}
                        </span>
                    </div>

                    {(card.verificationStale ||
                        card.needsVerification ||
                        card.status !== 'Operational') && (
                        <div
                            role="note"
                            className="flex gap-2 rounded-lg border border-[#f59e0b] bg-[#fff7e6] p-3 text-[#5c3a00] dark:bg-[#3a2a00] dark:text-[#ffd98a]"
                        >
                            <HiExclamation
                                aria-hidden
                                className="mt-0.5 shrink-0 text-lg"
                            />
                            <div className="flex flex-col gap-1">
                                {card.status !== 'Operational' && (
                                    <span>
                                        {tEnums(`status.${card.status}`)}
                                    </span>
                                )}
                                {card.needsVerification && (
                                    <span>{t('card.needsVerification')}</span>
                                )}
                                {card.verificationStale && (
                                    <span>{t('card.stale')}</span>
                                )}
                            </div>
                        </div>
                    )}

                    <div className="flex flex-wrap gap-2">
                        <Button
                            asElement="a"
                            href={directionsUrl(
                                card.latitude,
                                card.longitude,
                                userPosition,
                            )}
                            target="_blank"
                            rel="noopener noreferrer"
                            variant="solid"
                            icon={<HiExternalLink aria-hidden />}
                        >
                            {t('card.navigate')}
                        </Button>
                        {card.contactPhone && (
                            <Button
                                asElement="a"
                                href={`tel:${card.contactPhone}`}
                                variant="default"
                                icon={<HiOutlinePhone aria-hidden />}
                            >
                                {card.contactPhone}
                            </Button>
                        )}
                    </div>

                    <dl className="divide-y divide-gray-200 dark:divide-gray-800">
                        <Row label={t('card.address')}>
                            {card.address}, {card.postalCode} {card.city}
                            <br />
                            <span className="text-gray-700 dark:text-gray-300">
                                {card.municipality !== card.city &&
                                    `${card.municipality}, `}
                                {tEnums(`voivodeship.${card.voivodeship}`)}
                            </span>
                        </Row>
                        {card.placementDetails && (
                            <Row label={t('card.placement')}>
                                {card.placementDetails} (
                                {card.isIndoor
                                    ? t('card.indoor')
                                    : t('card.outdoor')}
                                )
                            </Row>
                        )}
                        <Row label={t('card.hours')}>
                            {card.availabilityHours ? (
                                // Read in the page language; the OpenStreetMap notation stays in the tooltip.
                                <span title={card.availabilityHours}>
                                    {describeOpeningHours(
                                        card.availabilityHours,
                                        locale,
                                        hoursLabels,
                                    )}
                                </span>
                            ) : (
                                t('card.noHours')
                            )}
                        </Row>
                        {card.accessNotes && (
                            <Row label={t('card.access')}>
                                {card.accessNotes}
                            </Row>
                        )}
                        <Row label={t('card.facility')}>
                            {tEnums(`facilityType.${card.facilityType}`)}
                        </Row>
                        <Row label={t('card.status')}>
                            {tEnums(`status.${card.status}`)}
                        </Row>
                        <Row label={t('card.coordinates')}>
                            <span className="font-mono">
                                {card.latitude.toFixed(6)},{' '}
                                {card.longitude.toFixed(6)}
                            </span>
                        </Row>
                        <Row label={t('card.owner')}>{card.ownerName}</Row>
                        <Row label={t('card.source')}>
                            {card.sourceUrl ? (
                                <a
                                    href={card.sourceUrl}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="text-[#1a56db] underline dark:text-[#8ab4ff]"
                                >
                                    {card.sourceName}
                                    <span className="sr-only">
                                        {' '}
                                        – {t('card.openSource')}
                                    </span>
                                </a>
                            ) : (
                                card.sourceName
                            )}
                        </Row>
                        <Row label={t('card.scope')}>{card.referenceScope}</Row>
                        <Row label={t('card.license')}>
                            {tEnums(`license.${card.license}`)}
                        </Row>
                        <Row label={t('card.verified')}>
                            {card.lastVerifiedAt
                                ? format.dateTime(
                                      new Date(card.lastVerifiedAt),
                                      { dateStyle: 'long' },
                                  )
                                : t('card.neverVerified')}
                        </Row>
                    </dl>

                    {/* key: a new card starts with a closed, empty form. */}
                    <ReportCorrection key={card.id} aedId={card.id} />
                </div>
            )}
        </Drawer>
    )
}

export default AedCard
