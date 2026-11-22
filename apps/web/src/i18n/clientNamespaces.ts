import type { MessageNamespace } from './messageNamespaces'

/**
 * Namespaces sent to the browser, per route segment (path under `app/[locale]`, `""` = the root layout).
 * The root layout passes its list to `NextIntlClientProvider`; every other segment has a `layout.tsx` with
 * `<PageMessages segment="...">`, which replaces the messages for the pages below it. The server always
 * has all messages (`getTranslations`). `clientNamespaces.test.ts` checks the lists against the code.
 * Pattern of Car Reminder (apps/web/src/i18n/clientNamespaces.ts).
 */
export const CLIENT_NAMESPACES = {
    '': ['common'],
    '(public)': ['aedMap', 'auth', 'common', 'enums'],
    panel: ['enums', 'panel'],
} as const satisfies Record<string, readonly MessageNamespace[]>

export type ClientSegment = Exclude<keyof typeof CLIENT_NAMESPACES, ''>

export function pickMessages<T extends Record<string, unknown>>(
    messages: T,
    namespaces: readonly string[],
): T {
    return Object.fromEntries(
        namespaces
            .filter((namespace) => namespace in messages)
            .map((namespace) => [namespace, messages[namespace]]),
    ) as T
}
