import { getPaged } from '@/shared/api'
import { get, post } from '@/shared/api/client'
import { INVENTORY } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  StockTransferQuery,
  StockTransferListItem,
  StockTransferDetail,
  StockTransferRequest,
  StockTransferReceiveRequest,
} from '../types/stockTransfer'
import {
  toStockTransferQueryParams,
  STOCK_TRANSFER_FILTER_FIELDS,
  STOCK_TRANSFER_SORT_FIELDS,
  STOCK_TRANSFER_SEARCH_FIELDS,
} from '../types/stockTransfer'

export class StockTransferApi {
  private static readonly BASE = `${INVENTORY}/stock-transfers`

  static getStockTransfers(query: StockTransferQuery): Promise<PagedResult<StockTransferListItem>> {
    return getPaged<StockTransferListItem>(StockTransferApi.BASE, toStockTransferQueryParams(query), {
      allowedFilterFields: STOCK_TRANSFER_FILTER_FIELDS,
      allowedSortFields: STOCK_TRANSFER_SORT_FIELDS,
      allowedSearchFields: STOCK_TRANSFER_SEARCH_FIELDS,
    })
  }

  static getStockTransfer(id: string): Promise<Result<StockTransferDetail>> {
    return get<Result<StockTransferDetail>>(`${StockTransferApi.BASE}/${id}`)
  }

  static createStockTransfer(request: StockTransferRequest): Promise<Result<StockTransferDetail>> {
    return post<Result<StockTransferDetail>>(StockTransferApi.BASE, request)
  }

  static transferStockTransfer(id: string): Promise<Result<void>> {
    return post<Result<void>>(`${StockTransferApi.BASE}/${id}/transfer`)
  }

  static receiveStockTransfer(id: string, request: StockTransferReceiveRequest): Promise<Result<void>> {
    return post<Result<void>>(`${StockTransferApi.BASE}/${id}/receive`, request)
  }

  static cancelStockTransfer(id: string): Promise<Result<void>> {
    return post<Result<void>>(`${StockTransferApi.BASE}/${id}/cancel`)
  }
}
