'use client'

import { useId, useState, type KeyboardEvent } from 'react'
import { HiOutlineSearch } from 'react-icons/hi'
import Button from '@/components/ui/Button'
import Dialog from '@/components/ui/Dialog'
import Tooltip from '@/components/ui/Tooltip'
import { INPUT_CLASS } from '@/components/panel/styles'

export type LookupOption = {
    value: string
    label: string
    /** Second column, e.g. the TERYT code or the holder (left out when no option has it). */
    extra?: string
}

export type LookupFieldProps = {
    id: string
    'aria-invalid': boolean
    'aria-describedby'?: string
    required?: boolean
}

/** Texts of the field and its window. */
export type LookupTexts = {
    /** Name of the magnifier, e.g. "Choose from the list: Voivodeship". */
    open: string
    /** Hidden label of the search box. */
    searchLabel: string
    /** Placeholder of the search box. */
    search: string
    /** Placeholder of the field when nothing is chosen. */
    placeholder: string
    nameHeader: string
    empty: string
    clear: string
    close: string
    /** Added to the chosen row for screen readers. */
    selected: string
    /** "3 of 8 shown" for screen readers (the screen shows "3 / 8"). */
    count: (shown: number, total: number) => string
}

type Props = {
    /** Name of the field: title of the window and part of the button names. */
    label: string
    value: string
    options: LookupOption[]
    onChange: (value: string) => void
    /** Header of the second column. */
    extraHeader?: string
    /** Optional fields: "Clear" in the window sets an empty value. */
    clearable?: boolean
    placeholder?: string
    /** id, aria-invalid, aria-describedby and required from FormField (the error under the field). */
    fieldProps: LookupFieldProps
    /** Texts in the page language (the caller's namespace), see `LookupTexts`. */
    texts: LookupTexts
}

/**
 * Choice from a list in the style of Car Reminder: the chosen value in a read-only field and a magnifier; both open
 * a window with a search box, a table and "x / y" shown. Keyboard: Enter, Space or Alt+↓ in the field opens it,
 * Tab moves through the rows, Enter picks, Esc closes (only the window, not the drawer around it); the focus
 * returns to the field. Rendered where it is used (e.g. inside a drawer), so clicks in the window stay "inside".
 * Shared by the panel (`components/panel/LookupSelect`) and the public map filters; the texts come from the caller.
 */
const LookupSelect = ({
    label,
    value,
    options,
    onChange,
    extraHeader,
    clearable,
    placeholder,
    fieldProps,
    texts,
}: Props) => {
    const titleId = useId()
    const searchId = useId()
    const [open, setOpen] = useState(false)
    const [search, setSearch] = useState('')

    const selected = options.find((option) => option.value === value)
    const hasExtra = !!extraHeader && options.some((option) => option.extra)
    const q = search.trim().toLocaleLowerCase()
    const filtered = q
        ? options.filter((option) =>
              `${option.label} ${option.extra ?? ''}`
                  .toLocaleLowerCase()
                  .includes(q),
          )
        : options

    const show = () => {
        setSearch('')
        setOpen(true)
    }

    const pick = (next: string) => {
        onChange(next)
        setOpen(false)
    }

    const onFieldKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
        if (
            event.key === 'Enter' ||
            event.key === ' ' ||
            (event.key === 'ArrowDown' && event.altKey)
        ) {
            event.preventDefault()
            show()
        }
    }

    // Esc closes this window only: stopped here, it does not reach the drawer's listener.
    const onWindowKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
        if (event.key === 'Escape') {
            event.stopPropagation()
            setOpen(false)
        }
    }

    const openLabel = texts.open

    return (
        <>
            <div className="flex gap-2">
                <input
                    {...fieldProps}
                    type="text"
                    readOnly
                    value={selected?.label ?? ''}
                    placeholder={placeholder ?? texts.placeholder}
                    aria-haspopup="dialog"
                    onClick={show}
                    onKeyDown={onFieldKeyDown}
                    className={`${INPUT_CLASS} min-w-0 flex-1 cursor-pointer`}
                />
                <Tooltip title={openLabel}>
                    <Button
                        type="button"
                        icon={<HiOutlineSearch aria-hidden />}
                        aria-label={openLabel}
                        aria-haspopup="dialog"
                        onClick={show}
                        className="h-11! w-11! shrink-0 rounded-xl! border-gray-500! focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                    />
                </Tooltip>
            </div>

            <Dialog
                isOpen={open}
                onClose={() => setOpen(false)}
                width={720}
                labelledBy={titleId}
                closeLabel={texts.close}
            >
                <div
                    onKeyDown={onWindowKeyDown}
                    className="flex flex-col gap-4"
                >
                    <div className="flex items-center justify-between gap-4 pr-10">
                        <h2
                            id={titleId}
                            className="text-lg font-bold text-gray-900 sm:text-xl dark:text-white"
                        >
                            {label}
                        </h2>
                        <p
                            aria-live="polite"
                            className="shrink-0 text-sm text-gray-600 dark:text-gray-400"
                        >
                            <span aria-hidden>
                                {filtered.length} / {options.length}
                            </span>
                            <span className="sr-only">
                                {texts.count(filtered.length, options.length)}
                            </span>
                        </p>
                    </div>

                    <div className="relative">
                        <label htmlFor={searchId} className="sr-only">
                            {texts.searchLabel}
                        </label>
                        <HiOutlineSearch
                            aria-hidden
                            className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-lg text-gray-500"
                        />
                        <input
                            id={searchId}
                            type="search"
                            autoFocus
                            autoComplete="off"
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            placeholder={texts.search}
                            className={`${INPUT_CLASS} pl-11`}
                        />
                    </div>

                    <div className="max-h-[50vh] overflow-auto rounded-xl border border-gray-200 dark:border-gray-700">
                        <table className="w-full border-collapse text-left text-sm">
                            <thead className="sticky top-0 bg-gray-50 dark:bg-gray-800">
                                <tr>
                                    <th
                                        scope="col"
                                        className="px-4 py-3 font-semibold text-gray-700 dark:text-gray-300"
                                    >
                                        {texts.nameHeader}
                                    </th>
                                    {hasExtra && (
                                        <th
                                            scope="col"
                                            className="px-4 py-3 font-semibold text-gray-700 dark:text-gray-300"
                                        >
                                            {extraHeader}
                                        </th>
                                    )}
                                </tr>
                            </thead>
                            <tbody>
                                {filtered.length === 0 ? (
                                    <tr>
                                        <td
                                            colSpan={hasExtra ? 2 : 1}
                                            className="px-4 py-6 text-center text-gray-600 dark:text-gray-400"
                                        >
                                            {texts.empty}
                                        </td>
                                    </tr>
                                ) : (
                                    filtered.map((option) => {
                                        const isSelected =
                                            option.value === value
                                        return (
                                            <tr
                                                key={option.value}
                                                onClick={() =>
                                                    pick(option.value)
                                                }
                                                className={`cursor-pointer border-t border-gray-100 first:border-t-0 dark:border-gray-700 ${isSelected ? 'bg-primary-subtle' : 'hover:bg-gray-50 dark:hover:bg-gray-700/50'}`}
                                            >
                                                <td className="p-0">
                                                    {/* The row is clicked with the mouse; the button gives it a keyboard stop and a name. */}
                                                    <button
                                                        type="button"
                                                        aria-current={
                                                            isSelected
                                                                ? 'true'
                                                                : undefined
                                                        }
                                                        onClick={(e) => {
                                                            e.stopPropagation()
                                                            pick(option.value)
                                                        }}
                                                        className={`block min-h-11 w-full px-4 py-3 text-left text-gray-900 focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-primary dark:text-gray-100 ${isSelected ? 'font-semibold' : ''}`}
                                                    >
                                                        {option.label}
                                                        {isSelected && (
                                                            <span className="sr-only">
                                                                {' '}
                                                                (
                                                                {texts.selected}
                                                                )
                                                            </span>
                                                        )}
                                                    </button>
                                                </td>
                                                {hasExtra && (
                                                    <td className="px-4 py-3 text-gray-600 dark:text-gray-400">
                                                        {option.extra}
                                                    </td>
                                                )}
                                            </tr>
                                        )
                                    })
                                )}
                            </tbody>
                        </table>
                    </div>

                    <div className="flex flex-wrap justify-between gap-2">
                        {clearable ? (
                            <Button
                                type="button"
                                disabled={!value}
                                onClick={() => pick('')}
                            >
                                {texts.clear}
                            </Button>
                        ) : (
                            <span />
                        )}
                        <Button type="button" onClick={() => setOpen(false)}>
                            {texts.close}
                        </Button>
                    </div>
                </div>
            </Dialog>
        </>
    )
}

export default LookupSelect
