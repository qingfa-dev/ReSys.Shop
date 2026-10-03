// Format: Default datetime display options — medium date + 24h time
const DATETIME_OPTIONS: Intl.DateTimeFormatOptions = {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
}

/**
 * Formats a UTC timestamp (ISO string or Date) into the user's local time zone.
 * Backend returns UTC timestamps; use this for every order timestamp rendered in the UI.
 */
export function formatDateTimeUtc(
  value: string | Date | null | undefined,
  options: Intl.DateTimeFormatOptions = DATETIME_OPTIONS,
  locale = 'vi-VN',
): string {
  // Guard: Return em-dash for null/undefined/invalid dates
  if (value === null || value === undefined) return '—'
  const date = typeof value === 'string' ? new Date(value) : value
  if (Number.isNaN(date.getTime())) return '—'
  return new Intl.DateTimeFormat(locale, options).format(date)
}
