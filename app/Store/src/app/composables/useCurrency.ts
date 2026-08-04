import { formatVnd } from '@/shared/utils/currency'

export function useCurrency(): {
  formatVnd: (amount: number) => string
} {
  return { formatVnd }
}
