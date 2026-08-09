import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { STOCK_TRANSFER_FILTER_FIELDS, STOCK_TRANSFER_SORT_FIELDS, STOCK_TRANSFER_SEARCH_FIELDS } from '../types/stockTransfer'
import type { StockTransferListItem } from '../types/stockTransfer'

export function useStockTransferList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockTransferListItem>(`api/admin/inventory/stock-transfers`, {
    allowedFilterFields: STOCK_TRANSFER_FILTER_FIELDS,
    allowedSortFields: STOCK_TRANSFER_SORT_FIELDS,
    allowedSearchFields: STOCK_TRANSFER_SEARCH_FIELDS,
    ...options,
  })
}
