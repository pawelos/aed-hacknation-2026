import js from '@eslint/js'
import tseslint from 'typescript-eslint'
import reactHooks from 'eslint-plugin-react-hooks'
import nextConfig from 'eslint-config-next/core-web-vitals'
import { defineConfig } from 'eslint/config'

export default defineConfig(
    { ignores: ['.next/**', 'src/icons/**', 'mcp-server/**'] },
    ...nextConfig,
    {
        extends: [js.configs.recommended, ...tseslint.configs.recommended],
        files: ['**/*.{ts,tsx}'],
        languageOptions: {
            ecmaVersion: 2020,
        },
        plugins: {
            'react-hooks': reactHooks,
        },
        rules: {
            ...reactHooks.configs.recommended.rules,
            'react-hooks/set-state-in-effect': 'off',
            'react-hooks/immutability': 'off',
            'react-hooks/refs': 'off',
            'react-hooks/incompatible-library': 'off',
            'no-useless-assignment': 'off',
            '@eslint/preserve-caught-error': 'off',
            'preserve-caught-error': 'off',
            'no-unassigned-vars': 'off',
            '@typescript-eslint/no-unused-vars': [
                'error',
                {
                    argsIgnorePattern: '^_',
                    varsIgnorePattern: '^_',
                    destructuredArrayIgnorePattern: '^_',
                    ignoreRestSiblings: true,
                },
            ],
        },
    },
    {
        // The next-intl request config and everything it calls run while next-intl resolves the
        // locale: next-intl (getLocale, getTranslations) or apiFetchAuthed (reads the locale) there
        // makes the config wait for itself, and every page of a signed-in user hangs.
        files: [
            'src/i18n/request.ts',
            'src/server/actions/locale.ts',
            'src/server/locale/clientDefaults.ts',
            'src/server/locale/resolveRegion.ts',
        ],
        rules: {
            'no-restricted-imports': [
                'error',
                {
                    paths: [
                        {
                            name: 'next-intl/server',
                            importNames: [
                                'getLocale',
                                'getMessages',
                                'getTranslations',
                                'getFormatter',
                                'getNow',
                                'getTimeZone',
                            ],
                            message:
                                'Runs inside the next-intl request config, which this would wait for (deadlock).',
                        },
                    ],
                    patterns: [
                        {
                            group: [
                                '@/services/api/server',
                                '@/services/api/localizeApiError',
                                '@/server/actions/*',
                                '!@/server/actions/locale',
                                '@/server/actions/*/**',
                            ],
                            message:
                                'Runs inside the next-intl request config: use apiFetch with the session token, not next-intl or apiFetchAuthed (deadlock).',
                        },
                    ],
                },
            ],
        },
    },
)
