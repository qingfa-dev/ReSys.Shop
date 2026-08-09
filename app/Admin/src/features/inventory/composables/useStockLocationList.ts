import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { STOCK_LOCATION_FILTER_FIELDS, STOCK_LOCATION_SORT_FIELDS, STOCK_LOCATION_SEARCH_FIELDS } from '../types/stockLocation'
import type { StockLocationListItem } from '../types/stockLocation'

export function useStockLocationList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockLocationListItem>(`api/admin/inventory/stock-locations`, {
    allowedFilterFields: STOCK_LOCATION_FILTER_FIELDS,
    allowedSortFields: STOCK_LOCATION_SORT_FIELDS,
    allowedSearchFields: STOCK_LOCATION_SEARCH_FIELDS,
    ...options,
  })
}
