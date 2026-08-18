import type { QueryingParameters } from '@/shared/types/querying'

export interface StockItemRequest {
  stockLocationId: string
  variantId: string
  countOnHand: number
  backorderable: boolean
}

export interface StockItemListItem extends StockItemRequest {
  id: string
  createdAtUtc: string
  modifiedAtUtc: string | null
  createdBy: string | null
  modifiedBy: string | null
}

export type StockItemDetail = StockItemListItem

export interface StockItemQuery {
  stockLocationId?: string
  variantId?: string
  backorderable?: boolean
  sortBy?: 'countOnHand' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const STOCK_ITEM_FILTER_FIELDS = ['stockLocationId', 'variantId', 'backorderable']

export const STOCK_ITEM_SORT_FIELDS = ['countOnHand', 'createdAtUtc']

export const STOCK_ITEM_SEARCH_FIELDS: string[] = []

export function toStockItemQueryParams(query: StockItemQuery): QueryingParameters {
  const filters: string[] = []

  if (query.stockLocationId !== undefined && query.stockLocationId !== '') {
    filters.push(`stockLocationId=${query.stockLocationId}`)
  }
  if (query.variantId !== undefined && query.variantId !== '') {
    filters.push(`variantId=${query.variantId}`)
  }
  if (query.backorderable === true) {
    filters.push('backorderable=true')
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}

export interface BulkAdjustItem {
  stockItemId: string
  quantity: number
}

export interface BulkAdjustStockItemsRequest {
  stockLocationId: string
  variantId: string
  countOnHand: number
  backorderable: boolean
  items: BulkAdjustItem[]
  reason?: string
}

export interface RestockRequest {
  quantity: number
  reference?: string
  reason?: string
}

export interface RestockResultResponse {
  stockItemId: string
  previousCountOnHand: number
  newCountOnHand: number
  backordersFulfilled: number
  partiallyFulfilled: boolean
  remainingQuantity: number
  movementId: string | null
}

export interface LocationBreakdownItem {
  locationId: string
  locationName: string
  countOnHand: number
  reserved: number
  available: number
  isLowStock: boolean
}

export interface StockSummaryDetailResponse {
  variantId: string
  totalOnHand: number
  totalReserved: number
  totalAvailable: number
  locationBreakdown: LocationBreakdownItem[]
}

export interface ImportStockItemsResponse {
  created: number
  updated: number
  failed: number
  errors: string[]
}

export interface LowStockQuery {
  locationId?: string
  threshold?: number
  page?: number
  pageSize?: number
}

export interface LowStockItem extends StockItemListItem {
  locationName: string
  threshold: number
  status: 'Low' | 'OutOfStock'
}
