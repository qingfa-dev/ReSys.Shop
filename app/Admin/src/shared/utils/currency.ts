export function formatCurrency(value: number | null | undefined, currency = 'USD', locale = 'en-US'): string {
  if (value === null || value === undefined) return '$0.00'
  const safe = currency?.trim() || 'USD'
  return new Intl.NumberFormat(locale, { style: 'currency', currency: safe }).format(value)
}

export function parseCurrency(value: string): number {
  return parseFloat(value.replace(/[^0-9.-]/g, '')) || 0
}
