import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { STOCK_ITEM_FILTER_FIELDS, STOCK_ITEM_SORT_FIELDS, STOCK_ITEM_SEARCH_FIELDS } from '../types/stockItem'
import type { StockItemListItem } from '../types/stockItem'

export function useStockItemList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockItemListItem>(`api/admin/inventory/stock-items`, {
    allowedFilterFields: STOCK_ITEM_FILTER_FIELDS,
    allowedSortFields: STOCK_ITEM_SORT_FIELDS,
    allowedSearchFields: STOCK_ITEM_SEARCH_FIELDS,
    ...options,
  })
}
