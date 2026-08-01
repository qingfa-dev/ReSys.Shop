import type { QueryingParameters } from '@/shared/types/querying'

export interface StockMovementListItem {
  id: string
  stockItemId: string
  quantity: number
  previousCountOnHand: number
  action?: string
  reason?: string
  originatorType?: string
  originatorId?: string
  createdAtUtc: string
}

export type StockMovementDetail = StockMovementListItem

export interface StockMovementQuery {
  fromUtc?: string
  toUtc?: string
  variantId?: string
  stockLocationId?: string
  sortBy?: 'quantity' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export interface StockMovementQueryParams extends QueryingParameters {
  fromUtc: string | null
  toUtc: string | null
  variantId: string | null
  stockLocationId: string | null
}

export const STOCK_MOVEMENT_FILTER_FIELDS = ['StockItemId', 'OriginatorType']

export const STOCK_MOVEMENT_SORT_FIELDS = ['Quantity', 'CreatedAtUtc']

export const STOCK_MOVEMENT_SEARCH_FIELDS = ['Reason']

export function toStockMovementQueryParams(query: StockMovementQuery): StockMovementQueryParams {
  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: null,
    search: null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
    fromUtc: query.fromUtc ?? null,
    toUtc: query.toUtc ?? null,
    variantId: query.variantId ?? null,
    stockLocationId: query.stockLocationId ?? null,
  }
}
