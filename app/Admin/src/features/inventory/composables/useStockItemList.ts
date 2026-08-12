import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { StockItemApi } from '../services/stockItemApi'
import type { StockItemListItem } from '../types/stockItem'

export function useStockItemList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockItemListItem>((params) => StockItemApi.getStockItems(params), {
    ...options,
  })
}