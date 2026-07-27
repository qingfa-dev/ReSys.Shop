import { useI18n } from 'vue-i18n'

export function useCurrency() {
  const { locale } = useI18n()

  function format(value: number | null | undefined, currency = 'USD'): string {
    if (value === null || value === undefined) return '$0.00'
    return new Intl.NumberFormat(locale.value, {
      style: 'currency',
      currency,
    }).format(value)
  }

  return { format }
}
