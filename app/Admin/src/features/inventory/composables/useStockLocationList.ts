import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { StockLocationApi } from '../services/stockLocationApi'
import type { StockLocationListItem } from '../types/stockLocation'

export function useStockLocationList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockLocationListItem>((params) => StockLocationApi.getStockLocations(params), {
    ...options,
  })
}