/** Enum values as sent by the API (names, the same as in the database and the export). */
export const AED_AVAILABILITIES = ['Public24h', 'BusinessHours', 'Restricted'] as const
export type AedAvailability = (typeof AED_AVAILABILITIES)[number]

export const AED_STATUSES = ['Operational', 'ToVerify', 'Outdated', 'Withdrawn'] as const
export type AedStatus = (typeof AED_STATUSES)[number]

export const FACILITY_TYPES = [
    'PublicOffice',
    'School',
    'Healthcare',
    'FireStation',
    'Police',
    'Shop',
    'Office',
    'Transport',
    'Sport',
    'Religious',
    'CultureAndTourism',
    'OutdoorCabinet',
    'Other',
] as const
export type FacilityType = (typeof FACILITY_TYPES)[number]

export type DataLicense = 'Cc0' | 'CcBy4'

/** Point on the map (`AedMapPoint` of the API). */
export type AedMapPoint = {
    id: string
    latitude: number
    longitude: number
    locationName: string
    city: string
    address: string
    availability: AedAvailability
    status: AedStatus
    isOpenNow: boolean | null
}

export type AedMapResponse = { items: AedMapPoint[]; truncated: boolean }

export type NearestAed = { point: AedMapPoint; distanceMeters: number }

export type AedSearchResponse = {
    items: AedMapPoint[]
    centerLatitude: number | null
    centerLongitude: number | null
}

/** Public card of one AED (`AedCardResponse` of the API). */
export type AedCard = {
    id: string
    locationName: string
    address: string
    postalCode: string
    city: string
    municipality: string
    county: string
    voivodeship: string
    latitude: number
    longitude: number
    placementDetails: string
    isIndoor: boolean
    facilityType: FacilityType
    availability: AedAvailability
    availabilityHours: string
    accessNotes: string
    status: AedStatus
    contactPhone: string
    isOpenNow: boolean | null
    lastVerifiedAt: string | null
    verificationStale: boolean
    needsVerification: boolean
    ownerName: string
    sourceName: string
    sourceUrl: string
    license: DataLicense
    referenceScope: string
}

/** Envelope of every API answer (`Result<T>`). */
export type ApiResult<T> = {
    value: T | null
    isSuccess: boolean
    error: {
        code: string
        description: string
        fields?: Record<string, string>
    }
}

/** Visible map area. */
export type Bounds = {
    minLat: number
    minLng: number
    maxLat: number
    maxLng: number
}

/** Filters of the map and the list (kept in the address so a link can be shared). */
export type AedFilters = {
    availability: AedAvailability[]
    facilityType: FacilityType | ''
    openNow: boolean
}
