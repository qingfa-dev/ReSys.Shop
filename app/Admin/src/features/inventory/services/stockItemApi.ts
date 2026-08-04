import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { INVENTORY } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  StockItemRequest,
  StockItemListItem,
  StockItemDetail,
  StockItemQuery,
  BulkAdjustStockItemsRequest,
  RestockRequest,
  RestockResultResponse,
  LowStockQuery,
  LowStockItem,
  StockSummaryDetailResponse,
  ImportStockItemsResponse,
} from '../types/stockItem'
import {
  toStockItemQueryParams,
  STOCK_ITEM_FILTER_FIELDS,
  STOCK_ITEM_SORT_FIELDS,
  STOCK_ITEM_SEARCH_FIELDS,
} from '../types/stockItem'

export class StockItemApi {
  private static readonly BASE = `${INVENTORY}/stock-items`

  static getStockItems(query: StockItemQuery): Promise<PagedResult<StockItemListItem>> {
    return getPaged<StockItemListItem>(StockItemApi.BASE, toStockItemQueryParams(query), {
      allowedFilterFields: STOCK_ITEM_FILTER_FIELDS,
      allowedSortFields: STOCK_ITEM_SORT_FIELDS,
      allowedSearchFields: STOCK_ITEM_SEARCH_FIELDS,
    })
  }

  static getStockItem(id: string): Promise<Result<StockItemDetail>> {
    return get<Result<StockItemDetail>>(`${StockItemApi.BASE}/${id}`)
  }

  static createStockItem(request: StockItemRequest): Promise<Result<StockItemDetail>> {
    return post<Result<StockItemDetail>>(StockItemApi.BASE, request)
  }

  static updateStockItem(id: string, request: StockItemRequest): Promise<Result<StockItemDetail>> {
    return put<Result<StockItemDetail>>(`${StockItemApi.BASE}/${id}`, request)
  }

  static deleteStockItem(id: string): Promise<Result<StockItemListItem>> {
    return del<Result<StockItemListItem>>(`${StockItemApi.BASE}/${id}`)
  }

  static bulkAdjustStockItems(request: BulkAdjustStockItemsRequest): Promise<Result<void>> {
    return post<Result<void>>(`${StockItemApi.BASE}/bulk-adjust`, request)
  }

  static restockStockItem(id: string, request: RestockRequest): Promise<Result<RestockResultResponse>> {
    return post<Result<RestockResultResponse>>(`${StockItemApi.BASE}/${id}/restock`, request)
  }

  static getLowStockItems(params: LowStockQuery): Promise<PagedResult<LowStockItem>> {
    const parts: string[] = []
    if (params.locationId !== undefined) parts.push(`locationId=${params.locationId}`)
    if (params.threshold !== undefined) parts.push(`threshold=${params.threshold}`)
    const url = `${StockItemApi.BASE}/low-stock${parts.length ? `?${parts.join('&')}` : ''}`
    return getPaged<LowStockItem>(url, {
      pageNumber: params.page ?? null,
      pageSize: params.pageSize ?? null,
    })
  }

  static getStockSummary(query: { page?: number; pageSize?: number }): Promise<PagedResult<StockSummaryDetailResponse>> {
    return getPaged<StockSummaryDetailResponse>(`${StockItemApi.BASE}/summary`, {
      pageNumber: query.page ?? null,
      pageSize: query.pageSize ?? null,
    })
  }

  static importStockItems(file: File): Promise<Result<ImportStockItemsResponse>> {
    const formData = new FormData()
    formData.append('file', file)
    return post<Result<ImportStockItemsResponse>>(`${StockItemApi.BASE}/import`, formData)
  }
}
