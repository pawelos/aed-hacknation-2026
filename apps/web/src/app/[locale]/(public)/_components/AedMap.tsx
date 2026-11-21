'use client'

import { useEffect, useRef } from 'react'
import { useTranslations } from 'next-intl'
import L from 'leaflet'
import 'leaflet.markercluster'
import { MapContainer, Marker, TileLayer, useMap, useMapEvents } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'
import 'leaflet.markercluster/dist/MarkerCluster.css'
import 'leaflet.markercluster/dist/MarkerCluster.Default.css'
import { OSM_TILE_ORIGIN } from '@/constants/csp.constant'
import type { AedMapPoint, Bounds } from '@/services/aed/types'
import { pinHtml } from './AedMarkerIcon'
import { isDimmed } from './aedFormat'

/** South-west and north-east corners of Poland. */
const POLAND_BOUNDS: L.LatLngBoundsExpression = [
    [49.0, 14.12],
    [54.84, 24.15],
]

/** A place to move the map to; a new `key` moves it again to the same place. */
export type FlyTarget = { latitude: number; longitude: number; zoom: number; key: number }

type AedMapProps = {
    points: AedMapPoint[]
    selectedId: string | null
    userPosition: { latitude: number; longitude: number } | null
    flyTo: FlyTarget | null
    onBoundsChange: (bounds: Bounds) => void
    onSelect: (id: string) => void
}

const userIcon = L.divIcon({
    className: 'aed-pin-wrap',
    html: '<span class="aed-user-dot" aria-hidden="true"></span>',
    iconSize: [18, 18],
    iconAnchor: [9, 9],
})

function toBounds(map: L.Map): Bounds {
    const b = map.getBounds()
    return {
        minLat: b.getSouth(),
        minLng: b.getWest(),
        maxLat: b.getNorth(),
        maxLng: b.getEast(),
    }
}

/** Reports the visible area after each move and once at the start. */
function BoundsWatcher({ onBoundsChange }: { onBoundsChange: (bounds: Bounds) => void }) {
    const map = useMapEvents({
        moveend: () => onBoundsChange(toBounds(map)),
    })
    useEffect(() => {
        onBoundsChange(toBounds(map))
        // Once, when the map is ready.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [map])
    return null
}

/** Moves the map when the target changes (search result, nearest AED, a row picked in the list). */
function FlyController({ target }: { target: FlyTarget | null }) {
    const map = useMap()
    useEffect(() => {
        if (target) {
            map.flyTo([target.latitude, target.longitude], target.zoom, { duration: 0.8 })
        }
    }, [map, target])
    return null
}

/**
 * Pins grouped in clusters (leaflet.markercluster), so the whole country stays readable. Each pin is
 * reachable with the keyboard (Leaflet gives it tabindex and role="button") and named by its title.
 */
function ClusterLayer({ points, selectedId, onSelect }: Pick<AedMapProps, 'points' | 'selectedId' | 'onSelect'>) {
    const map = useMap()
    const groupRef = useRef<L.MarkerClusterGroup | null>(null)
    const onSelectRef = useRef(onSelect)
    onSelectRef.current = onSelect

    useEffect(() => {
        const group = L.markerClusterGroup({ showCoverageOnHover: false, maxClusterRadius: 50 })
        groupRef.current = group
        map.addLayer(group)
        return () => {
            map.removeLayer(group)
            groupRef.current = null
        }
    }, [map])

    useEffect(() => {
        const group = groupRef.current
        if (!group) {
            return
        }
        group.clearLayers()
        const markers = points.map((point) => {
            const selected = point.id === selectedId
            const marker = L.marker([point.latitude, point.longitude], {
                icon: L.divIcon({
                    className: selected ? 'aed-pin-wrap aed-pin-wrap--selected' : 'aed-pin-wrap',
                    html: pinHtml(point.availability, isDimmed(point)),
                    iconSize: [32, 32],
                    iconAnchor: [16, 16],
                }),
                title: `${point.locationName}, ${point.address}, ${point.city}`,
                keyboard: true,
                riseOnHover: true,
                zIndexOffset: selected ? 1000 : 0,
            })
            marker.on('click', () => onSelectRef.current(point.id))
            marker.on('keypress', (e: L.LeafletKeyboardEvent) => {
                if (e.originalEvent.key === 'Enter' || e.originalEvent.key === ' ') {
                    onSelectRef.current(point.id)
                }
            })
            return marker
        })
        group.addLayers(markers)
    }, [points, selectedId])

    return null
}

/** Leaflet map with OpenStreetMap tiles. Loaded only in the browser (next/dynamic, ssr: false). */
const AedMap = ({ points, selectedId, userPosition, flyTo, onBoundsChange, onSelect }: AedMapProps) => {
    const t = useTranslations('aedMap')

    return (
        // isolate: Leaflet panes use z-index 400-1000; kept inside the map so the card drawer covers them.
        <div role="region" aria-label={t('map.label')} className="isolate h-full w-full">
            <MapContainer
                // The whole of Poland fits at the start, also on a narrow phone screen.
                bounds={POLAND_BOUNDS}
                minZoom={5}
                maxZoom={19}
                className="h-full w-full"
                worldCopyJump={false}
            >
                <TileLayer
                    url={`${OSM_TILE_ORIGIN}/{z}/{x}/{y}.png`}
                    attribution={`<a href="https://www.openstreetmap.org/copyright">${t('map.attribution')}</a>`}
                    maxZoom={19}
                />
                <BoundsWatcher onBoundsChange={onBoundsChange} />
                <FlyController target={flyTo} />
                <ClusterLayer points={points} selectedId={selectedId} onSelect={onSelect} />
                {userPosition && (
                    <Marker
                        position={[userPosition.latitude, userPosition.longitude]}
                        icon={userIcon}
                        title={t('nearest.you')}
                        keyboard={false}
                    />
                )}
            </MapContainer>
        </div>
    )
}

export default AedMap
