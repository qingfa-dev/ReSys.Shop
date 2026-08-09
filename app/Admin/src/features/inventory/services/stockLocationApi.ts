import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult } from '@/shared/types'
import type {
  StockLocationRequest,
  StockLocationListItem,
  StockLocationDetail,
  StockLocationQuery,
} from '../types/stockLocation'
import {
  toStockLocationQueryParams,
  STOCK_LOCATION_FILTER_FIELDS,
  STOCK_LOCATION_SORT_FIELDS,
  STOCK_LOCATION_SEARCH_FIELDS,
} from '../types/stockLocation'

export class StockLocationApi {
  private static readonly BASE = 'api/admin/inventory/stock-locations'

  static getStockLocations(query: StockLocationQuery): Promise<PagedResult<StockLocationListItem>> {
    return getPaged<StockLocationListItem>(StockLocationApi.BASE, toStockLocationQueryParams(query), {
      allowedFilterFields: STOCK_LOCATION_FILTER_FIELDS,
      allowedSortFields: STOCK_LOCATION_SORT_FIELDS,
      allowedSearchFields: STOCK_LOCATION_SEARCH_FIELDS,
    })
  }

  static getStockLocation(id: string): Promise<Result<StockLocationDetail>> {
    return get<Result<StockLocationDetail>>(`${StockLocationApi.BASE}/${id}`)
  }

  static createStockLocation(request: StockLocationRequest): Promise<Result<StockLocationDetail>> {
    return post<Result<StockLocationDetail>>(StockLocationApi.BASE, request)
  }

  static updateStockLocation(id: string, request: StockLocationRequest): Promise<Result<StockLocationDetail>> {
    return put<Result<StockLocationDetail>>(`${StockLocationApi.BASE}/${id}`, request)
  }

  static deleteStockLocation(id: string): Promise<Result<StockLocationListItem>> {
    return del<Result<StockLocationListItem>>(`${StockLocationApi.BASE}/${id}`)
  }

  static setDefaultStockLocation(id: string): Promise<Result<StockLocationDetail>> {
    return put<Result<StockLocationDetail>>(`${StockLocationApi.BASE}/${id}/default`)
  }
}
