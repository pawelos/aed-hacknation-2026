import type { AedMapPoint } from '@/services/aed/types'

/** Distance for people: metres below 1 km, else kilometres with one decimal. */
export function formatDistance(meters: number, locale: string): string {
    if (meters < 1000) {
        // Rounded to 10 m (GPS is not more precise); a device a few metres away is "10 m", not "0 m".
        return `${Math.max(10, Math.round(meters / 10) * 10)} m`
    }
    return `${new Intl.NumberFormat(locale, { maximumFractionDigits: 1 }).format(meters / 1000)} km`
}

/** True when the device should be drawn as not usable right now (closed, or data to be checked). */
export function isDimmed(point: Pick<AedMapPoint, 'isOpenNow' | 'status'>): boolean {
    return point.isOpenNow === false || point.status !== 'Operational'
}

/** Directions on OpenStreetMap: on foot from the user's position when known, else the place itself. */
export function directionsUrl(
    latitude: number,
    longitude: number,
    from?: { latitude: number; longitude: number } | null,
): string {
    if (from) {
        return `https://www.openstreetmap.org/directions?engine=fossgis_osrm_foot&route=${from.latitude}%2C${from.longitude}%3B${latitude}%2C${longitude}`
    }
    return `https://www.openstreetmap.org/?mlat=${latitude}&mlon=${longitude}#map=18/${latitude}/${longitude}`
}
