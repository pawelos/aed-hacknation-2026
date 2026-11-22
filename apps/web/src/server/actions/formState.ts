import type { ApiResponse } from '@/server/session'

/**
 * Result of a form server action: field errors shown under their fields (API error.fields, camelCase keys as in
 * the form), a message for errors not about one field, or success. Pattern of Car Reminder (docs/form-validation.md).
 */
export type FormState = {
    ok: boolean
    /** Code of the API error (e.g. Auth.InvalidCredentials), for a translated message. */
    code?: string
    fieldErrors?: Record<string, string>
    message?: string
    /** Bumped on every submit, so the form can react to the same result twice. */
    at?: number
}

/** Field errors of the API first, the rest as a message. */
export function fromApiError(response: ApiResponse<unknown>): FormState {
    const fields = response.error?.fields
    const hasFields = !!fields && Object.keys(fields).length > 0
    return {
        ok: false,
        code: response.error?.code,
        fieldErrors: hasFields ? fields : undefined,
        message: hasFields ? undefined : (response.error?.description ?? 'Error'),
        at: Date.now(),
    }
}
