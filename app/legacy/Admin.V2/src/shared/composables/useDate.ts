import { useI18n } from 'vue-i18n'

export function useDate() {
  const { locale } = useI18n()

  function format(value: string | Date | null | undefined, options?: Intl.DateTimeFormatOptions): string {
    if (!value) return '-'
    const date = typeof value === 'string' ? new Date(value) : value
    return new Intl.DateTimeFormat(locale.value, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      ...options,
    }).format(date)
  }

  function formatRelative(value: string | Date | null | undefined): string {
    if (!value) return '-'
    const date = typeof value === 'string' ? new Date(value) : value
    const now = new Date()
    const diffMs = now.getTime() - date.getTime()
    const diffSec = Math.floor(diffMs / 1000)
    const diffMin = Math.floor(diffSec / 60)
    const diffHour = Math.floor(diffMin / 60)
    const diffDay = Math.floor(diffHour / 24)

    if (diffSec < 60) return 'just now'
    if (diffMin < 60) return `${diffMin}m ago`
    if (diffHour < 24) return `${diffHour}h ago`
    if (diffDay < 7) return `${diffDay}d ago`
    return format(date)
  }

  return { format, formatRelative }
}
