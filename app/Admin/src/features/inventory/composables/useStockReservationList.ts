import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { StockReservationApi } from '../services/stockReservationApi'
import type { StockReservationListItem } from '../types/stockReservation'

export function useStockReservationList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockReservationListItem>((params) => StockReservationApi.getStockReservations(params), {
    ...options,
  })
}