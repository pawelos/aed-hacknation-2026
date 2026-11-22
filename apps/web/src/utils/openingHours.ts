/** Words of an opening_hours value that are not day or month names (translated by the caller). */
export type OpeningHoursLabels = {
    /** "24/7" */
    always: string
    /** "PH" – public holidays */
    holidays: string
    /** "off" / "closed" */
    closed: string
}

const DAYS = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'] as const
const MONTHS = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'May',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Oct',
    'Nov',
    'Dec',
] as const

/** Short names of the days (Monday first) and months in the given language, e.g. "pon.", "sty". */
function localNames(locale: string) {
    const day = new Intl.DateTimeFormat(locale, {
        weekday: 'short',
        timeZone: 'UTC',
    })
    const month = new Intl.DateTimeFormat(locale, {
        month: 'short',
        timeZone: 'UTC',
    })
    return {
        // 1 January 2024 was a Monday.
        days: DAYS.map((_, i) => day.format(Date.UTC(2024, 0, 1 + i))),
        months: MONTHS.map((_, i) => month.format(Date.UTC(2024, i, 1))),
    }
}

/**
 * Reads an OpenStreetMap opening_hours value (English abbreviations, e.g. "Mo-Fr 08:00-16:00; Sa 10:00-14:00")
 * in the given language: "pon.–pt. 08:00–16:00; sob. 10:00–14:00". Day and month names come from Intl, the other
 * words from `labels`; anything else (times, comments) stays as written. Empty input gives an empty string.
 */
export function describeOpeningHours(
    value: string,
    locale: string,
    labels: OpeningHoursLabels,
): string {
    const text = value.trim()
    if (!text) {
        return ''
    }
    const names = localNames(locale)
    return (
        text
            .replace(/\b24\/7\b/g, labels.always)
            .replace(
                /\b(Mo|Tu|We|Th|Fr|Sa|Su|PH|Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|off|closed)\b/g,
                (token) => {
                    const day = DAYS.indexOf(token as (typeof DAYS)[number])
                    if (day >= 0) return names.days[day]
                    const month = MONTHS.indexOf(
                        token as (typeof MONTHS)[number],
                    )
                    if (month >= 0) return names.months[month]
                    switch (token) {
                        case 'PH':
                            return labels.holidays
                        case 'off':
                        case 'closed':
                            return labels.closed
                        default:
                            return token
                    }
                },
            )
            // Ranges with an en dash: "08:00–16:00", "pon.–pt.".
            .replace(/(\S)-(\S)/g, '$1–$2')
    )
}
