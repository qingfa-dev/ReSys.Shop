import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  StockItemRequest,
  StockItemListItem,
  StockItemDetail,
  BulkAdjustStockItemsRequest,
  RestockRequest,
  RestockResultResponse,
  LowStockQuery,
  LowStockItem,
  StockSummaryDetailResponse,
  ImportStockItemsResponse,
} from '../types/stockItem'
import {
  STOCK_ITEM_FILTER_FIELDS,
  STOCK_ITEM_SORT_FIELDS,
  STOCK_ITEM_SEARCH_FIELDS,
} from '../types/stockItem'

export class StockItemApi {
  static getStockItems(params: QueryingParameters): Promise<PagedResult<StockItemListItem>> {
    return getPaged<StockItemListItem>('/api/admin/inventory/stock-items', params, {
      allowedFilterFields: STOCK_ITEM_FILTER_FIELDS,
      allowedSortFields: STOCK_ITEM_SORT_FIELDS,
      allowedSearchFields: STOCK_ITEM_SEARCH_FIELDS,
    })
  }

  static getStockItem(id: string): Promise<Result<StockItemDetail>> {
    return get<Result<StockItemDetail>>(`/api/admin/inventory/stock-items/${id}`)
  }

  static createStockItem(request: StockItemRequest): Promise<Result<StockItemDetail>> {
    return post<Result<StockItemDetail>>('/api/admin/inventory/stock-items', request)
  }

  static updateStockItem(id: string, request: StockItemRequest): Promise<Result<StockItemDetail>> {
    return put<Result<StockItemDetail>>(`/api/admin/inventory/stock-items/${id}`, request)
  }

  static deleteStockItem(id: string): Promise<Result<StockItemListItem>> {
    return del<Result<StockItemListItem>>(`/api/admin/inventory/stock-items/${id}`)
  }

  static bulkAdjustStockItems(request: BulkAdjustStockItemsRequest): Promise<Result<void>> {
    return post<Result<void>>('/api/admin/inventory/stock-items/bulk-adjust', request)
  }

  static restockStockItem(id: string, request: RestockRequest): Promise<Result<RestockResultResponse>> {
    return post<Result<RestockResultResponse>>(`/api/admin/inventory/stock-items/${id}/restock`, request)
  }

  static getLowStockItems(params: LowStockQuery): Promise<PagedResult<LowStockItem>> {
    const parts: string[] = []
    if (params.locationId !== undefined) parts.push(`locationId=${params.locationId}`)
    if (params.threshold !== undefined) parts.push(`threshold=${params.threshold}`)
    const url = `/api/admin/inventory/stock-items/low-stock${parts.length ? `?${parts.join('&')}` : ''}`
    return getPaged<LowStockItem>(url, {
      pageNumber: params.page ?? null,
      pageSize: params.pageSize ?? null,
    })
  }

  static getStockSummary(query: { page?: number; pageSize?: number }): Promise<PagedResult<StockSummaryDetailResponse>> {
    return getPaged<StockSummaryDetailResponse>('/api/admin/inventory/stock-items/summary', {
      pageNumber: query.page ?? null,
      pageSize: query.pageSize ?? null,
    })
  }

  static importStockItems(file: File): Promise<Result<ImportStockItemsResponse>> {
    const formData = new FormData()
    formData.append('file', file)
    return post<Result<ImportStockItemsResponse>>('/api/admin/inventory/stock-items/import', formData)
  }
}
