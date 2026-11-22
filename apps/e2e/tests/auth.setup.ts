import { test as setup } from '@playwright/test'
import { signInToPanel, storageStateOf, type Role } from './support/accounts'

/**
 * Signs in once per role and saves the session (httpOnly cookie `aed-session`), so panel specs start signed in.
 * Carries every tag: a `--grep @smoke` run must still run this setup first.
 */
for (const role of ['holder', 'admin'] as Role[]) {
    setup(`sign in as ${role}`, { tag: ['@smoke', '@critical'] }, async ({ page, context }) => {
        await signInToPanel(page, role)
        await context.storageState({ path: storageStateOf(role) })
    })
}
