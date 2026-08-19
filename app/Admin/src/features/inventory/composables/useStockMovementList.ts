import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { StockMovementApi } from '../services/stockMovementApi'
import type { StockMovementListItem, StockMovementQuery } from '../types/stockMovement'

export function useStockMovementList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockMovementListItem>((params) => StockMovementApi.getStockMovements(params as StockMovementQuery), {
    ...options,
  })
}