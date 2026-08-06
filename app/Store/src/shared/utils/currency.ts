import { usePreferences } from '@/shared/composables/usePreferences'

export function formatCurrency(amount: number): string {
  return usePreferences().formatCurrency(amount)
}
