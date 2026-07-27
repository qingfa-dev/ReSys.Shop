import { useToast } from '@/shared/composables/useToast'
import { StockMovementApi } from '../api'

export function useStockMovement() {
  const toast = useToast()
  return { toast, api: StockMovementApi }
}
