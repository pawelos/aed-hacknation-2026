/** One account of the "quick fill" on the sign-in page. */
export type DemoAccount = { key: 'wroclaw' | 'osp' | 'admin'; email: string; password: string }

/**
 * Demo accounts of the prototype for the sign-in quick fill (pattern of Car Reminder, but the passwords come from
 * the environment, not the code). Shown when DEMO_LOGIN is "true" (on by default in development); an account
 * without a password is left out. On the VPS the passwords are the ones the API seeder creates the accounts with
 * (aed.env: SEED_ADMIN_PASSWORD, SEED_DEMO_PASSWORD).
 */
export function getDemoAccounts(): DemoAccount[] {
    const isDev = process.env.NODE_ENV === 'development'
    const enabled = (process.env.DEMO_LOGIN ?? (isDev ? 'true' : 'false')).toLowerCase() === 'true'
    if (!enabled) {
        return []
    }

    // Development defaults: api/Aed/Api/appsettings.Development.json (Seed:*).
    const adminPassword = process.env.DEMO_ADMIN_PASSWORD || (isDev ? 'AedAdmin2026' : '')
    const demoPassword = process.env.DEMO_PASSWORD || (isDev ? 'AedDemo2026x' : '')

    const accounts: DemoAccount[] = [
        { key: 'wroclaw', email: 'wroclaw@aed.local', password: demoPassword },
        { key: 'osp', email: 'osp@aed.local', password: demoPassword },
        { key: 'admin', email: process.env.DEMO_ADMIN_EMAIL || 'admin@aed.local', password: adminPassword },
    ]
    return accounts.filter((account) => account.password.length > 0)
}
