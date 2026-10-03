import { computed, ref } from 'vue'

export type CurrencyCode = 'SGD' | 'USD' | 'GBP' | 'EUR' | 'JPY'

const DEFAULT_CURRENCY: CurrencyCode = 'SGD'

const CURRENCY_CONFIG: Record<CurrencyCode, { locale: string; symbol: string }> = {
  SGD: { locale: 'en-SG', symbol: 'SGD' },
  USD: { locale: 'en-US', symbol: '$' },
  GBP: { locale: 'en-GB', symbol: '£' },
  EUR: { locale: 'de-DE', symbol: '€' },
  JPY: { locale: 'ja-JP', symbol: '¥' },
}

export function useCurrency(initialCurrency?: CurrencyCode) {
  const currency = ref<CurrencyCode>(initialCurrency || DEFAULT_CURRENCY)

  const config = computed(() => CURRENCY_CONFIG[currency.value] || CURRENCY_CONFIG[DEFAULT_CURRENCY])

  function setCurrency(newCurrency: CurrencyCode) {
    currency.value = newCurrency
  }

  function format(amount: number, currencyCode?: CurrencyCode): string {
    const cur = currencyCode || currency.value
    const conf = CURRENCY_CONFIG[cur] || CURRENCY_CONFIG[DEFAULT_CURRENCY]

    return new Intl.NumberFormat(conf.locale, {
      style: 'currency',
      currency: cur,
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(amount)
  }

  function formatCompact(amount: number, currencyCode?: CurrencyCode): string {
    const cur = currencyCode || currency.value
    
    if (amount >= 1000) {
      return `${format(Math.round(amount / 1000), cur)}k`
    }
    return format(amount, cur)
  }

  function formatWithoutCurrency(amount: number, currencyCode?: CurrencyCode): string {
    const cur = currencyCode || currency.value
    const conf = CURRENCY_CONFIG[cur] || CURRENCY_CONFIG[DEFAULT_CURRENCY]

    return new Intl.NumberFormat(conf.locale, {
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(amount)
  }

  return {
    currency,
    config,
    setCurrency,
    format,
    formatCompact,
    formatWithoutCurrency,
  }
}
