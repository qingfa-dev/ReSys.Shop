export function formatDate(
  value: string | Date | null | undefined,
  options?: Intl.DateTimeFormatOptions,
  locale = 'en-US',
): string {
  if (value === null || value === undefined) return '-'
  const date = typeof value === 'string' ? new Date(value) : value
  if (Number.isNaN(date.getTime())) return '-'
  return new Intl.DateTimeFormat(locale, options).format(date)
}

const DATETIME_OPTIONS: Intl.DateTimeFormatOptions = {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
}

/**
 * Formats a UTC timestamp (ISO string or Date) into the user's local time zone.
 * Use for every backend timestamp rendered in the UI — backend returns UTC.
 */
export function formatDateTimeUtc(
  value: string | Date | null | undefined,
  options: Intl.DateTimeFormatOptions = DATETIME_OPTIONS,
  locale = 'en-US',
): string {
  if (value === null || value === undefined) return '-'
  const date = typeof value === 'string' ? new Date(value) : value
  if (Number.isNaN(date.getTime())) return '-'
  return new Intl.DateTimeFormat(locale, options).format(date)
}

/**
 * Converts a local date/time value to a UTC ISO string before persisting.
 * Use for form inputs (date pickers return local "YYYY-MM-DD" or local time).
 */
export function toUtcIso(value: string | Date | null | undefined): string | null {
  if (value === null || value === undefined || value === '') return null
  const date = typeof value === 'string' ? new Date(value) : value
  if (Number.isNaN(date.getTime())) return null
  return date.toISOString()
}

/**
 * Converts a UTC timestamp to a local date-only input value ("YYYY-MM-DD").
 * Use to populate date pickers from backend UTC timestamps.
 */
export function fromUtcToDateInput(
  value: string | Date | null | undefined,
): string | null {
  if (value === null || value === undefined || value === '') return null
  const date = typeof value === 'string' ? new Date(value) : value
  if (Number.isNaN(date.getTime())) return null
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return local.toISOString().slice(0, 10)
}
