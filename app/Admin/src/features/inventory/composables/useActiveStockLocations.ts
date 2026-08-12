import { useActiveList } from '@/shared/composables'
import type { StockLocationListItem } from '../types/stockLocation'
import { StockLocationApi } from '../services/stockLocationApi'

export function useActiveStockLocations() {
  // Call: Inventory service — active stock locations for filter and form Selects
  return useActiveList<StockLocationListItem>(() =>
    StockLocationApi.getStockLocations({ sort: ['name'], pageSize: 100 }),
  )
}