import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  StockLocationRequest,
  StockLocationListItem,
  StockLocationDetail,
} from '../types/stockLocation'
import {
  STOCK_LOCATION_FILTER_FIELDS,
  STOCK_LOCATION_SORT_FIELDS,
  STOCK_LOCATION_SEARCH_FIELDS,
} from '../types/stockLocation'

export class StockLocationApi {
  static getStockLocations(params: QueryingParameters): Promise<PagedResult<StockLocationListItem>> {
    return getPaged<StockLocationListItem>('/api/admin/inventory/stock-locations', params, {
      allowedFilterFields: STOCK_LOCATION_FILTER_FIELDS,
      allowedSortFields: STOCK_LOCATION_SORT_FIELDS,
      allowedSearchFields: STOCK_LOCATION_SEARCH_FIELDS,
    })
  }

  static getStockLocation(id: string): Promise<Result<StockLocationDetail>> {
    return get<Result<StockLocationDetail>>(`/api/admin/inventory/stock-locations/${id}`)
  }

  static createStockLocation(request: StockLocationRequest): Promise<Result<StockLocationDetail>> {
    return post<Result<StockLocationDetail>>('/api/admin/inventory/stock-locations', request)
  }

  static updateStockLocation(id: string, request: StockLocationRequest): Promise<Result<StockLocationDetail>> {
    return put<Result<StockLocationDetail>>(`/api/admin/inventory/stock-locations/${id}`, request)
  }

  static deleteStockLocation(id: string): Promise<Result<StockLocationListItem>> {
    return del<Result<StockLocationListItem>>(`/api/admin/inventory/stock-locations/${id}`)
  }

  static setDefaultStockLocation(id: string): Promise<Result<StockLocationDetail>> {
    return put<Result<StockLocationDetail>>(`/api/admin/inventory/stock-locations/${id}/default`)
  }
}