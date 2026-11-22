'use client'

import { useActionState, useState, type FormEvent } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import Button from '@/components/ui/Button'
import FormField from '@/components/panel/FormField'
import { INPUT_CLASS } from '@/components/panel/styles'
import { INPUT_LIMITS } from '@/constants/input-limits.constant'
import { loginAction } from '@/server/actions/auth'
import type { FormState } from '@/server/actions/formState'
import type { DemoAccount } from '@/server/demoAccounts'

/** API codes of sign-in errors with their own translated text. */
const TRANSLATED_CODES: Record<string, string> = {
    'Auth.InvalidCredentials': 'invalidCredentials',
    'Auth.LockedOut': 'lockedOut',
}

/**
 * E-mail and password; errors of the form and of the API under their fields. The inputs are controlled: React
 * resets an uncontrolled form after its action, which would clear the e-mail after a wrong password.
 */
const SignInForm = ({ next, demoAccounts }: { next: string; demoAccounts: DemoAccount[] }) => {
    const t = useTranslations('auth')
    const locale = useLocale()
    const [state, formAction, pending] = useActionState<FormState, FormData>(loginAction.bind(null, locale), { ok: false })
    // Like Car Reminder: with demo accounts the first one is filled in, so "Sign in" works at once.
    const [demoKey, setDemoKey] = useState<string>(demoAccounts[0]?.key ?? '')
    const [email, setEmail] = useState(demoAccounts[0]?.email ?? '')
    const [password, setPassword] = useState(demoAccounts[0]?.password ?? '')
    const [clientErrors, setClientErrors] = useState<Record<string, string>>({})
    const [submittedAt, setSubmittedAt] = useState(0)

    const applyDemoAccount = (key: string) => {
        const account = demoAccounts.find((a) => a.key === key)
        if (!account) {
            return
        }
        setDemoKey(key)
        setEmail(account.email)
        setPassword(account.password)
        setClientErrors({})
        // Errors of the previous attempt no longer apply: hide the last server result.
        setSubmittedAt((state.at ?? 0) + 1)
    }

    // Server errors belong to the last submit; a new client check replaces them.
    const serverErrors = state.at && state.at >= submittedAt ? (state.fieldErrors ?? {}) : {}
    const translated = state.code && TRANSLATED_CODES[state.code] ? t(TRANSLATED_CODES[state.code]) : null
    const errors: Record<string, string> = { ...serverErrors, ...clientErrors }
    if (translated && serverErrors.password) {
        errors.password = translated
    }
    const message = state.at && state.at >= submittedAt && !state.fieldErrors ? (translated ?? state.message) : null

    const validate = (event: FormEvent<HTMLFormElement>) => {
        const found: Record<string, string> = {}
        if (!email.trim()) {
            found.email = t('required')
        } else if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email.trim())) {
            found.email = t('invalidEmail')
        }
        if (!password) {
            found.password = t('required')
        }
        setClientErrors(found)
        if (Object.keys(found).length > 0) {
            event.preventDefault()
            return
        }
        setSubmittedAt(Date.now() - 1)
    }

    return (
        <form action={formAction} onSubmit={validate} noValidate className="flex flex-col gap-4">
            <input type="hidden" name="next" value={next} />
            {demoAccounts.length > 0 && (
                <div role="group" aria-labelledby="quick-fill-title" className="flex flex-col gap-2">
                    <p id="quick-fill-title" className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                        {t('quickFill.title')}
                    </p>
                    {/* Equal columns that fit the card at any width (the Eyris Segment does not shrink its items). */}
                    <div
                        className="grid gap-1 rounded-lg bg-gray-100 p-1 dark:bg-gray-800"
                        style={{ gridTemplateColumns: `repeat(${demoAccounts.length}, minmax(0, 1fr))` }}
                    >
                        {demoAccounts.map((account) => (
                            <button
                                key={account.key}
                                type="button"
                                aria-pressed={demoKey === account.key}
                                onClick={() => applyDemoAccount(account.key)}
                                className="rounded-md px-2 py-1.5 text-xs font-semibold leading-tight text-gray-600 hover:text-gray-900 focus-visible:outline-2 focus-visible:outline-primary aria-pressed:bg-white aria-pressed:text-gray-900 aria-pressed:shadow-sm sm:text-sm dark:text-gray-300 dark:hover:text-white dark:aria-pressed:bg-gray-900 dark:aria-pressed:text-white"
                            >
                                {t(`quickFill.${account.key}`)}
                            </button>
                        ))}
                    </div>
                </div>
            )}
            <FormField label={t('email')} error={errors.email} required>
                {(props) => (
                    <input
                        {...props}
                        name="email"
                        type="email"
                        autoComplete="username"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        maxLength={INPUT_LIMITS.email}
                        className={INPUT_CLASS}
                    />
                )}
            </FormField>
            <FormField label={t('password')} error={errors.password} required>
                {(props) => (
                    <input
                        {...props}
                        name="password"
                        type="password"
                        autoComplete="current-password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        maxLength={INPUT_LIMITS.password}
                        className={INPUT_CLASS}
                    />
                )}
            </FormField>
            {message && (
                <p role="alert" className="text-sm text-[#b3261e] dark:text-[#ff8a80]">
                    {message}
                </p>
            )}
            <Button type="submit" variant="solid" loading={pending} block>
                {t('submit')}
            </Button>
        </form>
    )
}

export default SignInForm
