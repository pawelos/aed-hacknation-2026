import { test } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync, readdirSync, readFileSync, statSync } from 'node:fs'
import { dirname, join, relative, resolve, sep } from 'node:path'
import { fileURLToPath } from 'node:url'
import { CLIENT_NAMESPACES } from './clientNamespaces.ts'

/**
 * Checks `CLIENT_NAMESPACES` against the code: for each route segment, the namespaces used by
 * `useTranslation(s)` in its files and in everything they import (segments with their own entry
 * excluded). A failure prints the list to put in `clientNamespaces.ts`.
 */

const SRC = resolve(dirname(fileURLToPath(import.meta.url)), '..')
const APP = join(SRC, 'app', '[locale]')
const EXTENSIONS = ['.ts', '.tsx', '.js', '.jsx', '/index.ts', '/index.tsx']

/** Files that call `useTranslations()` without a namespace, with the namespaces of the keys they read. */
const ROOT_TRANSLATORS: Record<string, string[]> = {}

/** Read from the messages object directly, not through a hook. */
const ALSO_NEEDED: Record<string, string[]> = {}

type FileInfo = { imports: string[]; namespaces: string[] }

const fileInfos = new Map<string, FileInfo>()

function resolveImport(from: string, specifier: string): string | null {
    let base: string
    if (specifier.startsWith('@/')) {
        base = join(SRC, specifier.slice(2))
    } else if (specifier.startsWith('.')) {
        base = resolve(dirname(from), specifier)
    } else {
        return null
    }
    if (existsSync(base) && statSync(base).isFile()) {
        return base
    }
    const withExtension = EXTENSIONS.map((extension) => base + extension).find(
        existsSync,
    )
    return withExtension ?? null
}

function fileInfo(file: string): FileInfo {
    const cached = fileInfos.get(file)
    if (cached) {
        return cached
    }
    const source = readFileSync(file, 'utf8')
    const imports = [
        ...source.matchAll(/(?:from|import)\s*\(?\s*['"]([^'"]+)['"]/g),
    ]
        .map((match) => resolveImport(file, match[1]))
        .filter((path): path is string => path !== null)
    const namespaces = [
        ...source.matchAll(/useTranslations?\(\s*['"]([A-Za-z]+)/g),
    ].map((match) => match[1])
    if (/useTranslations?\(\s*\)/.test(source)) {
        const key = relative(SRC, file).split(sep).join('/')
        namespaces.push(
            ...(ROOT_TRANSLATORS[key] ?? [
                `useTranslations() without namespace in ${key}`,
            ]),
        )
    }
    const info = { imports, namespaces }
    fileInfos.set(file, info)
    return info
}

function usedNamespaces(entries: string[]): Set<string> {
    const seen = new Set<string>()
    const namespaces = new Set<string>()
    const stack = [...entries]
    while (stack.length > 0) {
        const file = stack.pop()!
        if (seen.has(file)) {
            continue
        }
        seen.add(file)
        const info = fileInfo(file)
        info.namespaces.forEach((namespace) => namespaces.add(namespace))
        stack.push(...info.imports)
    }
    return namespaces
}

const segmentOf = (dir: string) => relative(APP, dir).split(sep).join('/')

/** Source files of a segment, without the folders of segments that have their own entry. */
function segmentFiles(dir: string): string[] {
    return readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
        const path = join(dir, entry.name)
        if (entry.isDirectory()) {
            return segmentOf(path) in CLIENT_NAMESPACES
                ? []
                : segmentFiles(path)
        }
        return /\.(tsx?|jsx?)$/.test(entry.name) &&
            !entry.name.includes('.test.')
            ? [path]
            : []
    })
}

for (const [segment, declared] of Object.entries(CLIENT_NAMESPACES)) {
    const label = segment || '[locale]'

    test(`${label}: the layout passes these namespaces to the client`, () => {
        // Without whitespace, so line breaks added by the formatter do not matter.
        const layout = readFileSync(
            join(APP, segment, 'layout.tsx'),
            'utf8',
        ).replace(/\s+/g, '')
        const marker =
            segment === ''
                ? "pickMessages(messages,CLIENT_NAMESPACES['']"
                : `<PageMessagessegment="${segment}">`
        assert.ok(
            layout.includes(marker),
            `${label}/layout.tsx must contain ${marker}`,
        )
    })

    test(`${label}: lists the namespaces its client components use`, () => {
        const used = usedNamespaces(segmentFiles(join(APP, segment)))
        ALSO_NEEDED[segment]?.forEach((namespace) => used.add(namespace))
        const expected = [...used].sort()
        assert.deepEqual([...declared].sort(), expected)
    })
}
