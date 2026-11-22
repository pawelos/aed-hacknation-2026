/**
 * maxLength of the form fields = MaximumLength of the API validators (AedDeviceConstant, CorrectionReportConstant,
 * AuthConstant). Pattern of Car Reminder (INPUT_LIMITS, docs/form-validation.md): change both together.
 */
export const INPUT_LIMITS = {
    email: 256,
    password: 128,
    sourceRecordId: 100,
    locationName: 300,
    address: 300,
    postalCode: 6,
    city: 100,
    municipality: 100,
    municipalityTeryt: 7,
    county: 100,
    placementDetails: 500,
    availabilityHours: 200,
    accessNotes: 500,
    contactPhone: 20,
    coordinate: 12,
    reportMessage: 2000,
    reporterContact: 200,
    resolutionNote: 1000,
    search: 100,
} as const
