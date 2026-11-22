'use server'

import { revalidatePath } from 'next/cache'
import { apiFetch } from '@/server/session'
import type { ImportReport } from '@/services/panel/types'
import { fromApiError, type FormState } from './formState'

/** Adds (no id) or changes an AED record. */
export async function saveAedAction(
    input: Record<string, unknown>,
): Promise<FormState & { id?: string }> {
    const isNew = !input.id
    const response = await apiFetch<string>(
        isNew ? '/api/v1/AedDevice/create' : '/api/v1/AedDevice/update',
        {
            method: isNew ? 'POST' : 'PUT',
            json: input,
        },
    )
    if (!response.ok) {
        return fromApiError(response)
    }
    revalidatePath('/[locale]/panel', 'layout')
    return {
        ok: true,
        id: isNew ? (response.value ?? undefined) : String(input.id),
        at: Date.now(),
    }
}

/** The owner confirms the record is up to date. */
export async function verifyAedAction(id: string): Promise<FormState> {
    const response = await apiFetch(
        `/api/v1/AedDevice/${encodeURIComponent(id)}/verify`,
        { method: 'POST' },
    )
    if (!response.ok) {
        return fromApiError(response)
    }
    revalidatePath('/[locale]/panel', 'layout')
    return { ok: true, at: Date.now() }
}

/** Removes a record from the data set. */
export async function deleteAedAction(id: string): Promise<FormState> {
    const response = await apiFetch(
        `/api/v1/AedDevice/${encodeURIComponent(id)}`,
        { method: 'DELETE' },
    )
    if (!response.ok) {
        return fromApiError(response)
    }
    revalidatePath('/[locale]/panel', 'layout')
    return { ok: true, at: Date.now() }
}

/**
 * Removes the chosen records in one request (the selection toolbar). All or nothing: when any record is missing,
 * the API changes nothing and says how many were not found.
 */
export async function batchDeleteAedsAction(
    ids: string[],
): Promise<FormState & { count?: number }> {
    const response = await apiFetch<number>('/api/v1/AedDevice/batch-delete', {
        method: 'POST',
        json: { ids },
    })
    if (!response.ok) {
        return fromApiError(response)
    }
    revalidatePath('/[locale]/panel', 'layout')
    return { ok: true, count: response.value ?? ids.length, at: Date.now() }
}

/** The owner confirms the chosen records are up to date, in one request; all or nothing like the removal. */
export async function batchVerifyAedsAction(
    ids: string[],
): Promise<FormState & { count?: number }> {
    const response = await apiFetch<number>('/api/v1/AedDevice/batch-verify', {
        method: 'POST',
        json: { ids },
    })
    if (!response.ok) {
        return fromApiError(response)
    }
    revalidatePath('/[locale]/panel', 'layout')
    return { ok: true, count: response.value ?? ids.length, at: Date.now() }
}

/** Accepts or rejects a correction report. */
export async function resolveReportAction(
    id: string,
    accept: boolean,
    note: string,
): Promise<FormState> {
    const response = await apiFetch(
        `/api/v1/CorrectionReport/${encodeURIComponent(id)}/resolve`,
        {
            method: 'PUT',
            json: { accept, resolutionNote: note },
        },
    )
    if (!response.ok) {
        return fromApiError(response)
    }
    revalidatePath('/[locale]/panel', 'layout')
    return { ok: true, at: Date.now() }
}

/** Sends the chosen file to the import (dry run or for real). */
export async function importAction(
    formData: FormData,
): Promise<FormState & { report?: ImportReport }> {
    const response = await apiFetch<ImportReport>('/api/v1/AedImport/file', {
        method: 'POST',
        form: formData,
    })
    if (!response.ok || !response.value) {
        return fromApiError(response)
    }
    if (!response.value.dryRun) {
        revalidatePath('/[locale]/panel', 'layout')
    }
    return { ok: true, report: response.value, at: Date.now() }
}
