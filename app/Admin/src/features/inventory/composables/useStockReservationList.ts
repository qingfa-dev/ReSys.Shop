import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { INVENTORY } from '@/shared/constants/api'
import { STOCK_RESERVATION_FILTER_FIELDS, STOCK_RESERVATION_SORT_FIELDS, STOCK_RESERVATION_SEARCH_FIELDS } from '../types/stockReservation'
import type { StockReservationListItem } from '../types/stockReservation'

export function useStockReservationList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockReservationListItem>(`${INVENTORY}/stock-reservations`, {
    allowedFilterFields: STOCK_RESERVATION_FILTER_FIELDS,
    allowedSortFields: STOCK_RESERVATION_SORT_FIELDS,
    allowedSearchFields: STOCK_RESERVATION_SEARCH_FIELDS,
    ...options,
  })
}
