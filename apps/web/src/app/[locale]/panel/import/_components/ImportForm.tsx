'use client'

import { useState, useTransition, type FormEvent } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter } from '@/i18n/navigation'
import Button from '@/components/ui/Button'
import FormField, { RequiredLegend } from '@/components/panel/FormField'
import LookupSelect from '@/components/panel/LookupSelect'
import {
    CARD_CLASS,
    GROUP_LABEL_CLASS,
    INPUT_CLASS,
    NOTICE_ERROR_CLASS,
    TABLE_WRAP_CLASS,
    TD_CLASS,
    TH_CLASS,
    THEAD_CLASS,
    TR_CLASS,
} from '@/components/panel/styles'
import { HiOutlineDocumentSearch, HiOutlineUpload } from 'react-icons/hi'
import { importAction } from '@/server/actions/panel'
import type { DataSource, ImportReport } from '@/services/panel/types'

/**
 * Source, file and options; "check" sends a dry run, "import" saves. The file stays chosen between the two, so the
 * holder imports exactly what was checked. Errors under the fields (API error.fields: dataSourceId, file).
 */
const ImportForm = ({ sources }: { sources: DataSource[] }) => {
    const t = useTranslations('panel')
    const router = useRouter()
    const [dataSourceId, setDataSourceId] = useState(
        sources.length === 1 ? sources[0].id : '',
    )
    const [file, setFile] = useState<File | null>(null)
    const [markMissing, setMarkMissing] = useState(false)
    const [errors, setErrors] = useState<Record<string, string>>({})
    const [message, setMessage] = useState<string | null>(null)
    const [report, setReport] = useState<ImportReport | null>(null)
    const [pending, startTransition] = useTransition()

    const send = (dryRun: boolean) => {
        const found: Record<string, string> = {}
        if (!dataSourceId) found.dataSourceId = t('form.required')
        if (!file) found.file = t('import.chooseFile')
        setErrors(found)
        setMessage(null)
        if (Object.keys(found).length > 0 || !file) {
            return
        }
        const data = new FormData()
        data.set('dataSourceId', dataSourceId)
        data.set('file', file)
        data.set('dryRun', String(dryRun))
        data.set('markMissingAsOutdated', String(markMissing))
        startTransition(async () => {
            const result = await importAction(data)
            if (result.ok && result.report) {
                setReport(result.report)
                if (!dryRun) {
                    router.refresh()
                }
                return
            }
            setErrors(result.fieldErrors ?? {})
            setMessage(result.message ?? null)
        })
    }

    const submit = (event: FormEvent) => {
        event.preventDefault()
        send(true)
    }

    const counters: [string, number][] = report
        ? [
              ['rows', report.totalRows],
              ['created', report.created],
              ['updated', report.updated],
              ['unchanged', report.unchanged],
              ['rejected', report.rejected],
              ['withWarnings', report.withWarnings],
              ['duplicates', report.possibleDuplicates],
              ['outdated', report.markedOutdated],
          ]
        : []
    const rows =
        report?.rows.filter(
            (r) =>
                r.errors.length > 0 ||
                r.warnings.length > 0 ||
                r.action !== 'Unchanged',
        ) ?? []

    return (
        <div className="flex flex-col gap-5">
            <form
                onSubmit={submit}
                noValidate
                className={`${CARD_CLASS} flex flex-col gap-5 p-4 sm:p-6`}
            >
                <RequiredLegend>{t('form.requiredLegend')}</RequiredLegend>
                {message && (
                    <p role="alert" className={NOTICE_ERROR_CLASS}>
                        {message}
                    </p>
                )}
                <fieldset className="grid min-w-0 gap-x-4 gap-y-5 md:grid-cols-2">
                    <legend className={`${GROUP_LABEL_CLASS} mb-3`}>
                        {t('import.sectionFile')}
                    </legend>
                    <FormField
                        label={t('import.source')}
                        error={errors.dataSourceId}
                        required
                    >
                        {(props) => (
                            <LookupSelect
                                label={t('import.source')}
                                fieldProps={props}
                                value={dataSourceId}
                                options={sources.map((source) => ({
                                    value: source.id,
                                    label: source.name,
                                    extra: source.ownerName,
                                }))}
                                extraHeader={t('form.colOwner')}
                                onChange={setDataSourceId}
                            />
                        )}
                    </FormField>
                    <FormField
                        label={t('import.file')}
                        error={errors.file}
                        required
                    >
                        {(props) => (
                            <input
                                {...props}
                                type="file"
                                accept=".csv,.txt,.geojson,.json,text/csv,application/geo+json,application/json"
                                onChange={(e) => {
                                    setFile(e.target.files?.[0] ?? null)
                                    setReport(null)
                                }}
                                className={`${INPUT_CLASS} file:mr-3 file:rounded-lg file:border-0 file:bg-gray-100 file:px-3 file:py-1.5 file:text-sm file:font-semibold file:text-gray-900 dark:file:bg-gray-700 dark:file:text-gray-100`}
                            />
                        )}
                    </FormField>
                    <label className="flex min-h-11 cursor-pointer items-center gap-3 text-sm text-gray-800 md:col-span-2 dark:text-gray-200">
                        <input
                            type="checkbox"
                            checked={markMissing}
                            onChange={(e) => setMarkMissing(e.target.checked)}
                            className="h-5 w-5 shrink-0 accent-[#00843a] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                        />
                        {t('import.markMissing')}
                    </label>
                </fieldset>
                {/* Footer as in the record drawer: the check on the left, the import (saves) on the right. */}
                <div className="flex flex-wrap items-center justify-between gap-2 border-t border-gray-200 pt-4 dark:border-gray-700">
                    <Button
                        type="submit"
                        variant="default"
                        icon={<HiOutlineDocumentSearch aria-hidden />}
                        loading={pending}
                    >
                        {t('import.check')}
                    </Button>
                    <Button
                        type="button"
                        variant="solid"
                        icon={<HiOutlineUpload aria-hidden />}
                        onClick={() => send(false)}
                        disabled={pending}
                    >
                        {t('import.run')}
                    </Button>
                </div>
            </form>

            {report && (
                <section
                    aria-labelledby="report-heading"
                    aria-live="polite"
                    className="flex flex-col gap-3"
                >
                    <h2
                        id="report-heading"
                        className="text-lg font-bold text-gray-900 dark:text-white"
                    >
                        {report.dryRun
                            ? t('import.reportDry')
                            : t('import.report')}{' '}
                        · {report.dataSourceName} · {report.fileName}
                    </h2>
                    <dl className="grid grid-cols-2 gap-2 sm:grid-cols-4">
                        {counters.map(([key, value]) => (
                            <div key={key} className={`${CARD_CLASS} p-4`}>
                                <dt className={GROUP_LABEL_CLASS}>
                                    {t(`import.${key}`)}
                                </dt>
                                <dd className="mt-1 text-2xl font-bold tabular-nums text-gray-900 dark:text-white">
                                    {value}
                                </dd>
                            </div>
                        ))}
                    </dl>
                    {report.unknownColumns.length > 0 && (
                        <p className="text-sm text-gray-700 dark:text-gray-300">
                            {t('import.unknownColumns', {
                                columns: report.unknownColumns.join(', '),
                            })}
                        </p>
                    )}
                    {rows.length === 0 ? (
                        <p className="text-sm text-gray-700 dark:text-gray-300">
                            {t('import.allRowsOk')}
                        </p>
                    ) : (
                        <div
                            role="region"
                            aria-labelledby="report-heading"
                            tabIndex={0}
                            className={TABLE_WRAP_CLASS}
                        >
                            <table className="w-full min-w-[720px] border-collapse text-left text-sm">
                                <thead className={THEAD_CLASS}>
                                    <tr className="border-b border-gray-200 dark:border-gray-800">
                                        <th scope="col" className={TH_CLASS}>
                                            {t('import.row')}
                                        </th>
                                        <th scope="col" className={TH_CLASS}>
                                            {t('import.action')}
                                        </th>
                                        <th scope="col" className={TH_CLASS}>
                                            {t('import.record')}
                                        </th>
                                        <th scope="col" className={TH_CLASS}>
                                            {t('import.issues')}
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {rows.map((row) => (
                                        <tr key={row.row} className={TR_CLASS}>
                                            <td
                                                className={`${TD_CLASS} tabular-nums`}
                                            >
                                                {row.row}
                                            </td>
                                            <td
                                                className={
                                                    'px-4 py-3 font-semibold ' +
                                                    (row.action === 'Rejected'
                                                        ? 'text-[#b3261e] dark:text-[#ff8a80]'
                                                        : 'text-gray-900 dark:text-white')
                                                }
                                            >
                                                {t(`import.${row.action}`)}
                                            </td>
                                            <td className={TD_CLASS}>
                                                {row.locationName ?? '–'}
                                                {row.sourceRecordId && (
                                                    <span className="block text-xs text-gray-600 dark:text-gray-400">
                                                        {row.sourceRecordId}
                                                    </span>
                                                )}
                                            </td>
                                            <td className={TD_CLASS}>
                                                <ul className="flex flex-col gap-1">
                                                    {row.errors.map(
                                                        (issue, i) => (
                                                            <li
                                                                key={`e${i}`}
                                                                className="text-[#b3261e] dark:text-[#ff8a80]"
                                                            >
                                                                {issue.field && (
                                                                    <code className="text-xs">
                                                                        {
                                                                            issue.field
                                                                        }
                                                                        :{' '}
                                                                    </code>
                                                                )}
                                                                {issue.message}
                                                            </li>
                                                        ),
                                                    )}
                                                    {row.warnings.map(
                                                        (issue, i) => (
                                                            <li
                                                                key={`w${i}`}
                                                                className="text-[#9a5b00] dark:text-[#ffc56b]"
                                                            >
                                                                {issue.field && (
                                                                    <code className="text-xs">
                                                                        {
                                                                            issue.field
                                                                        }
                                                                        :{' '}
                                                                    </code>
                                                                )}
                                                                {issue.message}
                                                            </li>
                                                        ),
                                                    )}
                                                </ul>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </section>
            )}
        </div>
    )
}

export default ImportForm
