import type { AedDetails, AedInput } from '@/services/panel/types'

/** Values of the form for a new record. */
export const EMPTY_AED: AedInput = {
    sourceRecordId: '',
    locationName: '',
    address: '',
    postalCode: '',
    city: '',
    municipality: '',
    municipalityTeryt: '',
    county: '',
    voivodeship: '',
    latitude: '',
    longitude: '',
    placementDetails: '',
    isIndoor: true,
    facilityType: 'Other',
    availability: 'BusinessHours',
    availabilityHours: '',
    accessNotes: '',
    status: 'Operational',
    contactPhone: '',
}

/** Values of the form for an existing record (coordinates as text with 6 decimals). */
export function aedToInput(aed: AedDetails): AedInput {
    return {
        sourceRecordId: aed.sourceRecordId,
        locationName: aed.locationName,
        address: aed.address,
        postalCode: aed.postalCode,
        city: aed.city,
        municipality: aed.municipality,
        municipalityTeryt: aed.municipalityTeryt,
        county: aed.county,
        voivodeship: aed.voivodeship,
        latitude: aed.latitude.toFixed(6),
        longitude: aed.longitude.toFixed(6),
        placementDetails: aed.placementDetails,
        isIndoor: aed.isIndoor,
        facilityType: aed.facilityType,
        availability: aed.availability,
        availabilityHours: aed.availabilityHours,
        accessNotes: aed.accessNotes,
        status: aed.status,
        contactPhone: aed.contactPhone,
    }
}
