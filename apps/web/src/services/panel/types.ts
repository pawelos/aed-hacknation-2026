import type { AedAvailability, AedStatus, FacilityType } from '@/services/aed/types'

export const VOIVODESHIPS = [
    'LowerSilesian',
    'KuyavianPomeranian',
    'Lublin',
    'Lubusz',
    'Lodz',
    'LesserPoland',
    'Masovian',
    'Opole',
    'Subcarpathian',
    'Podlaskie',
    'Pomeranian',
    'Silesian',
    'HolyCross',
    'WarmianMasurian',
    'GreaterPoland',
    'WestPomeranian',
] as const
export type Voivodeship = (typeof VOIVODESHIPS)[number]

export type DataSource = {
    id: string
    name: string
    ownerName: string
    url: string
    license: string
    referenceScope: string
    records: number
    lastVerifiedAt: string | null
}

export type PanelSummary = {
    sources: number
    records: number
    needsVerification: number
    verificationStale: number
    newReports: number
    lastImportAt: string | null
}

export type AedListItem = {
    id: string
    dataSourceId: string
    dataSourceName: string
    sourceRecordId: string
    locationName: string
    address: string
    city: string
    availability: AedAvailability
    status: AedStatus
    needsVerification: boolean
    lastVerifiedAt: string | null
    newReports: number
}

export type AedPage = { items: AedListItem[]; total: number; page: number; pageSize: number }

/** Editable fields (the panel form and the API commands). */
export type AedInput = {
    sourceRecordId: string
    locationName: string
    address: string
    postalCode: string
    city: string
    municipality: string
    municipalityTeryt: string
    county: string
    voivodeship: Voivodeship | ''
    latitude: string
    longitude: string
    placementDetails: string
    isIndoor: boolean
    facilityType: FacilityType
    availability: AedAvailability
    availabilityHours: string
    accessNotes: string
    status: AedStatus
    contactPhone: string
}

export type AedDetails = Omit<AedInput, 'latitude' | 'longitude' | 'voivodeship'> & {
    id: string
    dataSourceId: string
    dataSourceName: string
    voivodeship: Voivodeship
    latitude: number
    longitude: number
    lastVerifiedAt: string | null
    needsVerification: boolean
    verificationReason: string
    updatedAt: string | null
}

export type ImportIssue = { field: string | null; code: string; message: string }

export type ImportRow = {
    row: number
    action: 'Created' | 'Updated' | 'Unchanged' | 'Rejected'
    sourceRecordId: string | null
    locationName: string | null
    recordId: string | null
    errors: ImportIssue[]
    warnings: ImportIssue[]
    possibleDuplicateOf: string | null
}

export type ImportReport = {
    importId: string | null
    dryRun: boolean
    dataSourceName: string
    fileName: string
    format: string
    totalRows: number
    created: number
    updated: number
    unchanged: number
    rejected: number
    withWarnings: number
    possibleDuplicates: number
    markedOutdated: number
    unknownColumns: string[]
    rows: ImportRow[]
}

export type ImportSummary = {
    id: string
    dataSourceName: string
    format: string
    fileName: string
    createdAt: string
    totalRows: number
    created: number
    updated: number
    rejected: number
    possibleDuplicates: number
}

export type CorrectionReport = {
    id: string
    aedDeviceId: string
    locationName: string
    address: string
    city: string
    message: string
    reporterContact: string
    status: 'New' | 'Accepted' | 'Rejected'
    resolutionNote: string
    createdAt: string
    resolvedAt: string | null
}
