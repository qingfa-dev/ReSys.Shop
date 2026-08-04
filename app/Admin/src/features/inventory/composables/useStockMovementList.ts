import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { INVENTORY } from '@/shared/constants/api'
import { STOCK_MOVEMENT_FILTER_FIELDS, STOCK_MOVEMENT_SORT_FIELDS, STOCK_MOVEMENT_SEARCH_FIELDS } from '../types/stockMovement'
import type { StockMovementListItem } from '../types/stockMovement'

export function useStockMovementList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockMovementListItem>(`${INVENTORY}/stock-movements`, {
    allowedFilterFields: STOCK_MOVEMENT_FILTER_FIELDS,
    allowedSortFields: STOCK_MOVEMENT_SORT_FIELDS,
    allowedSearchFields: STOCK_MOVEMENT_SEARCH_FIELDS,
    ...options,
  })
}
