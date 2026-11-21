'use client'

import { useId, useState, type FormEvent } from 'react'
import { useTranslations } from 'next-intl'
import { HiOutlineSearch } from 'react-icons/hi'
import Input from '@/components/ui/Input'
import Button from '@/components/ui/Button'

/** Same limit as the API (PublicAedConstant.SearchMaxLength). */
const SEARCH_MAX_LENGTH = 100

type SearchBoxProps = {
    busy: boolean
    error: string | null
    onSearch: (query: string) => void
}

/** Search by town, municipality or address; the error is shown under the field. */
const SearchBox = ({ busy, error, onSearch }: SearchBoxProps) => {
    const t = useTranslations('aedMap')
    const inputId = useId()
    const errorId = useId()
    const [query, setQuery] = useState('')
    const [tooShort, setTooShort] = useState(false)

    const submit = (event: FormEvent) => {
        event.preventDefault()
        const text = query.trim()
        if (text.length < 2) {
            setTooShort(true)
            return
        }
        setTooShort(false)
        onSearch(text)
    }

    const message = tooShort ? t('search.tooShort') : error

    return (
        <form role="search" onSubmit={submit} className="flex min-w-0 flex-1 flex-col gap-1" noValidate>
            <label htmlFor={inputId} className="sr-only">
                {t('search.label')}
            </label>
            <div className="flex gap-2">
                <Input
                    id={inputId}
                    type="search"
                    value={query}
                    maxLength={SEARCH_MAX_LENGTH}
                    placeholder={t('search.placeholder')}
                    prefix={<HiOutlineSearch aria-hidden className="text-lg" />}
                    invalid={!!message}
                    aria-invalid={!!message}
                    aria-describedby={message ? errorId : undefined}
                    onChange={(e) => {
                        setQuery(e.target.value)
                        setTooShort(false)
                    }}
                />
                <Button type="submit" variant="solid" loading={busy}>
                    {t('search.submit')}
                </Button>
            </div>
            {message && (
                <p id={errorId} className="text-sm text-[#b3261e] dark:text-[#ff8a80]" role="alert">
                    {message}
                </p>
            )}
        </form>
    )
}

export default SearchBox
