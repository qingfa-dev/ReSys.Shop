export function useFormatter() {
  const formatCurrency = (value: number | null | undefined): string => {
    if (value === null || value === undefined) return '$0.00'
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(value)
  }

  const formatDate = (value: string | Date | null | undefined): string => {
    if (!value) return '-'
    const date = typeof value === 'string' ? new Date(value) : value
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(date)
  }

  const truncate = (text: string | null | undefined, length: number): string => {
    if (!text) return ''
    if (text.length <= length) return text
    return text.substring(0, length) + '...'
  }

  return {
    formatCurrency,
    formatDate,
    truncate,
  }
}
