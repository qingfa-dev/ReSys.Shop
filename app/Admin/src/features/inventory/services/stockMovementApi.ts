import { get } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult } from '@/shared/types'
import type {
  StockMovementQuery,
  StockMovementListItem,
  StockMovementDetail,
} from '../types/stockMovement'
import {
  toStockMovementQueryParams,
  STOCK_MOVEMENT_FILTER_FIELDS,
  STOCK_MOVEMENT_SORT_FIELDS,
  STOCK_MOVEMENT_SEARCH_FIELDS,
} from '../types/stockMovement'

export class StockMovementApi {
  private static readonly BASE = 'api/admin/inventory/stock-movements'

  static getStockMovements(query: StockMovementQuery): Promise<PagedResult<StockMovementListItem>> {
    const parts: string[] = []
    if (query.fromUtc !== undefined) parts.push(`fromUtc=${encodeURIComponent(query.fromUtc)}`)
    if (query.toUtc !== undefined) parts.push(`toUtc=${encodeURIComponent(query.toUtc)}`)
    if (query.variantId !== undefined) parts.push(`variantId=${query.variantId}`)
    if (query.stockLocationId !== undefined) parts.push(`stockLocationId=${query.stockLocationId}`)
    const url = `${StockMovementApi.BASE}${parts.length ? `?${parts.join('&')}` : ''}`

    const { fromUtc: _fromUtc, toUtc: _toUtc, variantId: _variantId, stockLocationId: _stockLocationId, ...rest } = query
    return getPaged<StockMovementListItem>(url, toStockMovementQueryParams(rest as StockMovementQuery), {
      allowedFilterFields: STOCK_MOVEMENT_FILTER_FIELDS,
      allowedSortFields: STOCK_MOVEMENT_SORT_FIELDS,
      allowedSearchFields: STOCK_MOVEMENT_SEARCH_FIELDS,
    })
  }

  static getStockMovement(id: string): Promise<Result<StockMovementDetail>> {
    return get<Result<StockMovementDetail>>(`${StockMovementApi.BASE}/${id}`)
  }
}
