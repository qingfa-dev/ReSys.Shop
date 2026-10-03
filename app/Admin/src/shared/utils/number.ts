export function formatNumber(value: number | null | undefined, decimals = 0, locale = 'en-US'): string {
  if (value === null || value === undefined) return '-'
  return new Intl.NumberFormat(locale, {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(value)
}
