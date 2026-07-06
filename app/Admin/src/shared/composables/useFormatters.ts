import { formatDate, formatCurrency, formatNumber } from '../lib/formatters'

export function useFormatters() {
  return { formatDate, formatCurrency, formatNumber }
}
