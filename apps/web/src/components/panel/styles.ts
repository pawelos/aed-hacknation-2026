/**
 * Class of text inputs, selects and textareas of the panel forms (shared by server and client components), in the
 * style of Car Reminder: 44 px tall (touch target), rounded, gray-500 border (3:1 against the background, WCAG 1.4.11).
 */
export const INPUT_CLASS =
    'w-full min-h-11 rounded-xl border border-gray-500 bg-white px-4 py-2.5 text-sm text-gray-900 placeholder:text-gray-500 focus-visible:border-primary focus-visible:outline-2 focus-visible:outline-offset-0 focus-visible:outline-primary aria-[invalid=true]:border-[#b3261e] aria-[invalid=true]:border-2 disabled:cursor-not-allowed disabled:bg-gray-100 dark:border-gray-500 dark:bg-gray-900 dark:text-gray-100 dark:placeholder:text-gray-400 dark:aria-[invalid=true]:border-[#ff8a80]'

/** White box of a panel page (filters, forms, tiles), as in Car Reminder. */
export const CARD_CLASS =
    'rounded-xl border border-gray-200 bg-white dark:border-gray-800 dark:bg-gray-900'

/** Small upper-case heading of a group (form sections, tile labels): "IDENTYFIKACJA". */
export const GROUP_LABEL_CLASS =
    'text-xs font-semibold uppercase tracking-wider text-gray-600 dark:text-gray-400'

/** Tables of the panel: a box that scrolls sideways, gray header with upper-case labels, roomy cells. */
export const TABLE_WRAP_CLASS = `overflow-x-auto ${CARD_CLASS} focus-visible:outline-2 focus-visible:outline-primary`
export const THEAD_CLASS = 'bg-gray-50 dark:bg-gray-800/60'
export const TH_CLASS =
    'px-4 py-3 text-xs font-semibold uppercase tracking-wider text-gray-700 dark:text-gray-300'
export const TD_CLASS = 'px-4 py-3 text-gray-700 dark:text-gray-300'
export const TR_CLASS =
    'border-b border-gray-100 align-top last:border-0 hover:bg-gray-50 dark:border-gray-800 dark:hover:bg-gray-800/40'

/** Links that look like buttons (navigation to another page or view). */
export const LINK_BUTTON_PRIMARY_CLASS =
    'inline-flex min-h-11 items-center gap-2 rounded-lg bg-primary px-5 text-sm font-semibold text-white hover:bg-primary-deep focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary'
export const LINK_BUTTON_DEFAULT_CLASS =
    'inline-flex min-h-11 items-center gap-2 rounded-lg border border-gray-300 bg-white px-5 text-sm font-semibold text-gray-900 hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100 dark:hover:bg-gray-800'

/** Text links inside sentences (underlined, 4.5:1). */
export const TEXT_LINK_CLASS =
    'text-[#1a56db] underline underline-offset-2 focus-visible:outline-2 focus-visible:outline-primary dark:text-[#8ab4ff]'

/** Messages over a list or a form: success (role="status"), error (role="alert"). */
export const NOTICE_SUCCESS_CLASS =
    'rounded-lg border border-[#00843a] bg-primary-subtle px-4 py-2 text-sm text-gray-900 dark:text-gray-100'
export const NOTICE_ERROR_CLASS =
    'rounded-lg border border-[#b3261e] bg-[#fdecea] px-4 py-2 text-sm text-[#8c1d18] dark:bg-[#3b1412] dark:text-[#ffb4ab]'
