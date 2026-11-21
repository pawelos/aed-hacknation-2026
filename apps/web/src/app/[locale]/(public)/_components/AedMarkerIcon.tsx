import classNames from 'classnames'
import type { AedAvailability } from '@/services/aed/types'

/**
 * Shape of a pin by availability, so it is told apart without colour (WCAG 1.4.1): circle = day and night,
 * rounded square = opening hours, diamond = restricted. The same shapes are used by the map (assets/styles/aed-map.css).
 */
export function pinClassName(availability: AedAvailability, dimmed: boolean): string {
    return classNames('aed-pin', `aed-pin--${availability.toLowerCase()}`, dimmed && 'aed-pin--dim')
}

/** HTML of a map pin (Leaflet divIcon); the lightning stands for the AED sign. */
export function pinHtml(availability: AedAvailability, dimmed: boolean): string {
    return `<span class="${pinClassName(availability, dimmed)}" aria-hidden="true"><span class="aed-pin__glyph">⚡</span></span>`
}

/** The same pin in the list and the legend. */
const AedMarkerIcon = ({ availability, dimmed = false }: { availability: AedAvailability; dimmed?: boolean }) => (
    <span className={pinClassName(availability, dimmed)} aria-hidden>
        <span className="aed-pin__glyph">⚡</span>
    </span>
)

export default AedMarkerIcon
