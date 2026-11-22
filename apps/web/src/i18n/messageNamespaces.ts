/**
 * Translation namespaces: one folder per namespace in `messages/`, one `<locale>.json` per language
 * (`messages/aedMap/pl.json`). A new namespace needs a folder with every language and an entry here.
 */
export const MESSAGE_NAMESPACES = ['common', 'enums', 'aedMap', 'openData', 'auth', 'panel'] as const

export type MessageNamespace = (typeof MESSAGE_NAMESPACES)[number]
