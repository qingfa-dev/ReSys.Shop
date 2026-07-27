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
