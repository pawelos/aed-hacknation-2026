/** Address of the API for server components (Aspire sets API_URL; locally 5296). */
function apiBaseUrl() {
    return (process.env.API_URL || 'http://localhost:5296').replace(/\/$/, '')
}

export type LocalizedText = { pl: string; en: string }

export type DatasetMetadata = {
    identifier: string
    title: LocalizedText
    description: LocalizedText
    keywordsPl: string[]
    keywordsEn: string[]
    category: string
    license: string
    licenseUrl: string
    updateFrequency: string
    publisher: string
    contactEmail: string
    landingPage: string
    issued: string
    modified: string | null
    records: number
    schemaVersion: string
    resources: {
        format: string
        mediaType: string
        downloadUrl: string
        title: LocalizedText
        description: LocalizedText
    }[]
    sources: {
        id: string
        name: string
        owner: string
        url: string
        license: string
        licenseUrl: string
        referenceScope: string
        records: number
        lastVerifiedAt: string | null
        csvUrl: string
    }[]
    fields: {
        name: string
        type: string
        required: boolean
        maxLength: number | null
        format: string
        description: LocalizedText
        example: string
        dictionary: string | null
    }[]
    dictionaries: { name: string; codes: { code: string; labelPl: string; labelEn: string }[] }[]
    templateUrl: string
    dcatUrl: string
}

/** Metadata of the open data set, read on the server (fresh on each request). */
export async function getDatasetMetadata(): Promise<DatasetMetadata | null> {
    try {
        const response = await fetch(`${apiBaseUrl()}/api/v1/OpenData/metadata.json`, {
            cache: 'no-store',
            signal: AbortSignal.timeout(10_000),
        })
        return response.ok ? ((await response.json()) as DatasetMetadata) : null
    } catch {
        return null
    }
}
