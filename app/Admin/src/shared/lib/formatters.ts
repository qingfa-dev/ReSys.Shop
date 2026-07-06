export function formatDate(d: Date | string, locale = 'en-US'): string {
  const date = typeof d === 'string' ? new Date(d) : d
  return new Intl.DateTimeFormat(locale, { year: 'numeric', month: '2-digit', day: '2-digit' }).format(date)
}

export function formatCurrency(amount: number, currency = 'USD', locale = 'en-US'): string {
  return new Intl.NumberFormat(locale, { style: 'currency', currency }).format(amount)
}

export function formatNumber(n: number, decimals = 2, locale = 'en-US'): string {
  return new Intl.NumberFormat(locale, { minimumFractionDigits: decimals, maximumFractionDigits: decimals }).format(n)
}
