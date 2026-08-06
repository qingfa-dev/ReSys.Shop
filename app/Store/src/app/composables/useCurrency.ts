import { formatCurrency } from '@/shared/utils/currency'

export function useCurrency(): {
  formatCurrency: (amount: number) => string
} {
  return { formatCurrency }
}
