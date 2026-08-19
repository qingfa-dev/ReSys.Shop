import { getPaged } from '@/shared/api'
import { get, post } from '@/shared/api/client'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  StockTransferListItem,
  StockTransferDetail,
  StockTransferRequest,
  StockTransferReceiveRequest,
} from '../types/stockTransfer'
import {
  STOCK_TRANSFER_FILTER_FIELDS,
  STOCK_TRANSFER_SORT_FIELDS,
  STOCK_TRANSFER_SEARCH_FIELDS,
} from '../types/stockTransfer'

export class StockTransferApi {
  static getStockTransfers(params: QueryingParameters): Promise<PagedResult<StockTransferListItem>> {
    return getPaged<StockTransferListItem>('/api/admin/inventory/stock-transfers', params, {
      allowedFilterFields: STOCK_TRANSFER_FILTER_FIELDS,
      allowedSortFields: STOCK_TRANSFER_SORT_FIELDS,
      allowedSearchFields: STOCK_TRANSFER_SEARCH_FIELDS,
    })
  }

  static getStockTransfer(id: string): Promise<Result<StockTransferDetail>> {
    return get<Result<StockTransferDetail>>(`/api/admin/inventory/stock-transfers/${id}`)
  }

  static createStockTransfer(request: StockTransferRequest): Promise<Result<StockTransferDetail>> {
    return post<Result<StockTransferDetail>>('/api/admin/inventory/stock-transfers', request)
  }

  static transferStockTransfer(id: string): Promise<Result<void>> {
    return post<Result<void>>(`/api/admin/inventory/stock-transfers/${id}/transfer`)
  }

  static receiveStockTransfer(id: string, request: StockTransferReceiveRequest): Promise<Result<void>> {
    return post<Result<void>>(`/api/admin/inventory/stock-transfers/${id}/receive`, request)
  }

  static cancelStockTransfer(id: string): Promise<Result<void>> {
    return post<Result<void>>(`/api/admin/inventory/stock-transfers/${id}/cancel`)
  }
}