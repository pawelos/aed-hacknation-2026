'use client'

import type { ReactNode } from 'react'
import ConfigProvider, { defaultConfig } from '@/components/ui/ConfigProvider'

/** Configuration of the Eyris UI kit (control size, direction, locale), as ThemeProvider in Car Reminder. */
const UiConfig = ({ locale, children }: { locale: string; children: ReactNode }) => (
    <ConfigProvider value={{ ...defaultConfig, locale }}>{children}</ConfigProvider>
)

export default UiConfig
