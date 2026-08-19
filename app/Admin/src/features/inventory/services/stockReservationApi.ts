import { getPaged } from '@/shared/api'
import { get, post } from '@/shared/api/client'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type { StockReservationListItem, StockReservationDetail } from '../types/stockReservation'
import {
  STOCK_RESERVATION_FILTER_FIELDS,
  STOCK_RESERVATION_SORT_FIELDS,
  STOCK_RESERVATION_SEARCH_FIELDS,
} from '../types/stockReservation'

export class StockReservationApi {
  static getStockReservations(params: QueryingParameters): Promise<PagedResult<StockReservationListItem>> {
    return getPaged<StockReservationListItem>('/api/admin/inventory/stock-reservations', params, {
      allowedFilterFields: STOCK_RESERVATION_FILTER_FIELDS,
      allowedSortFields: STOCK_RESERVATION_SORT_FIELDS,
      allowedSearchFields: STOCK_RESERVATION_SEARCH_FIELDS,
    })
  }

  static getStockReservation(id: string): Promise<Result<StockReservationDetail>> {
    return get<Result<StockReservationDetail>>(`/api/admin/inventory/stock-reservations/${id}`)
  }

  static cancelStockReservation(id: string): Promise<Result<StockReservationDetail>> {
    return post<Result<StockReservationDetail>>(`/api/admin/inventory/stock-reservations/${id}/cancel`)
  }
}