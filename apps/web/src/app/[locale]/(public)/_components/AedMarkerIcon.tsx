import classNames from 'classnames'
import type { AedAvailability } from '@/services/aed/types'

/**
 * Shape of a pin by availability, so it is told apart without colour (WCAG 1.4.1): circle = day and night,
 * rounded square = opening hours, diamond = restricted. The same shapes are used by the map (assets/styles/aed-map.css).
 */
export function pinClassName(availability: AedAvailability, dimmed: boolean): string {
    return classNames('aed-pin', `aed-pin--${availability.toLowerCase()}`, dimmed && 'aed-pin--dim')
}

/**
 * The AED sign (heart with a lightning bolt, as on the ISO 7010 E010 signs in buildings): the bolt is a hole in the
 * heart (evenodd), so it shows the pin's colour on every background. An SVG, not the ⚡ emoji, which systems draw
 * differently (a yellow colour emoji on Windows and Android).
 */
const AED_SIGN_SVG =
    '<svg class="aed-pin__glyph" viewBox="0 0 24 24" aria-hidden="true" focusable="false">' +
    '<path fill="currentColor" fill-rule="evenodd" d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35zM13.4 6.2 8.3 12.6h3.1l-1 5.2 5.3-6.7h-3.2z"/>' +
    '</svg>'

/** HTML of a map pin (Leaflet divIcon). */
export function pinHtml(availability: AedAvailability, dimmed: boolean): string {
    return `<span class="${pinClassName(availability, dimmed)}" aria-hidden="true">${AED_SIGN_SVG}</span>`
}

/** The same pin in the list and the legend. */
const AedMarkerIcon = ({ availability, dimmed = false }: { availability: AedAvailability; dimmed?: boolean }) => (
    <span
        className={pinClassName(availability, dimmed)}
        aria-hidden
        // Static markup shared with the Leaflet pins, no user data.
        dangerouslySetInnerHTML={{ __html: AED_SIGN_SVG }}
    />
)

export default AedMarkerIcon
