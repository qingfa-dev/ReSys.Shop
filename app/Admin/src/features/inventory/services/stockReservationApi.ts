import { getPaged } from '@/shared/api'
import { get, post } from '@/shared/api/client'

import type { Result, PagedResult } from '@/shared/types'
import type { StockReservationQuery, StockReservationListItem, StockReservationDetail } from '../types/stockReservation'
import {
  toStockReservationQueryParams,
  STOCK_RESERVATION_FILTER_FIELDS,
  STOCK_RESERVATION_SORT_FIELDS,
  STOCK_RESERVATION_SEARCH_FIELDS,
} from '../types/stockReservation'

export class StockReservationApi {
  private static readonly BASE = 'api/admin/inventory/stock-reservations'

  static getStockReservations(query: StockReservationQuery): Promise<PagedResult<StockReservationListItem>> {
    return getPaged<StockReservationListItem>(StockReservationApi.BASE, toStockReservationQueryParams(query), {
      allowedFilterFields: STOCK_RESERVATION_FILTER_FIELDS,
      allowedSortFields: STOCK_RESERVATION_SORT_FIELDS,
      allowedSearchFields: STOCK_RESERVATION_SEARCH_FIELDS,
    })
  }

  static getStockReservation(id: string): Promise<Result<StockReservationDetail>> {
    return get<Result<StockReservationDetail>>(`${StockReservationApi.BASE}/${id}`)
  }

  static cancelStockReservation(id: string): Promise<Result<StockReservationDetail>> {
    return post<Result<StockReservationDetail>>(`${StockReservationApi.BASE}/${id}/cancel`)
  }
}
