export function useFormatter() {
  const formatNumber = (value: number | null | undefined, decimals = 0): string => {
    if (value === null || value === undefined) return '-'
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
    }).format(value)
  }

  const truncate = (text: string | null | undefined, length: number): string => {
    if (!text) return ''
    if (text.length <= length) return text
    return text.substring(0, length) + '...'
  }

  return { formatNumber, truncate }
}
