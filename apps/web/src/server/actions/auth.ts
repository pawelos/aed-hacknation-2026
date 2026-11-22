'use server'

import { cookies } from 'next/headers'
import { redirect } from 'next/navigation'
import { apiFetch, SESSION_COOKIE } from '@/server/session'
import { fromApiError, type FormState } from './formState'

type LoginResponse = { accessToken: string; expiresAtUtc: string }

/** Only addresses inside the panel of this site (no open redirect). */
function safeNext(locale: string, next: FormDataEntryValue | null): string {
    const value = typeof next === 'string' ? next : ''
    return value.startsWith(`/${locale}/panel`) && !value.startsWith('//') ? value : `/${locale}/panel`
}

/** Signs in with e-mail and password; the API token goes to an httpOnly cookie. */
export async function loginAction(locale: string, _prev: FormState, formData: FormData): Promise<FormState> {
    const email = String(formData.get('email') ?? '').trim()
    const password = String(formData.get('password') ?? '')
    const response = await apiFetch<LoginResponse>('/api/v1/Auth/login', {
        method: 'POST',
        json: { email, password },
        anonymous: true,
    })
    if (!response.ok || !response.value) {
        return fromApiError(response)
    }

    const expires = new Date(response.value.expiresAtUtc)
    ;(await cookies()).set(SESSION_COOKIE, response.value.accessToken, {
        httpOnly: true,
        secure: process.env.NODE_ENV === 'production',
        sameSite: 'lax',
        path: '/',
        expires,
    })
    redirect(safeNext(locale, formData.get('next')))
}

/** Ends the session. */
export async function logoutAction(locale: string): Promise<void> {
    ;(await cookies()).delete(SESSION_COOKIE)
    redirect(`/${locale}/sign-in`)
}
