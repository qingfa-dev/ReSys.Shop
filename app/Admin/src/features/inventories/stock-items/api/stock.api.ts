import apiClient from '@/shared/api/http/api.client'
import { INVENTORY } from '@/shared/api/constants'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockItem, StockItemDetail, StockSummary } from '../types/StockItem.Response.Type'
import type { StockAdjustmentRequest } from '../types/StockItem.Request.Type'
import type { StockItemQuery } from '../types/StockItem.Query.Type'

function path(sub?: string): string {
  return `${INVENTORY}/stock-items${sub ? `/${sub}` : ''}`
}

export const stockRepository = {
  list(params: StockItemQuery): Promise<ServerPagedResult<StockItem>> {
    return apiClient.get(path(), { params }).then(res => res.data as ServerPagedResult<StockItem>)
  },
  getById(id: string): Promise<ServerResult<StockItemDetail>> {
    return apiClient.get(path(id)).then(res => res.data as ServerResult<StockItemDetail>)
  },
  create(data: { variantId: string; stockLocationId: string; countOnHand?: number }): Promise<ServerResult<StockItemDetail>> {
    return apiClient.post(path(), data).then(res => res.data as ServerResult<StockItemDetail>)
  },
  update(id: string, data: { countOnHand?: number; backorderable?: boolean; backorderLimit?: number }): Promise<ServerResult<void>> {
    return apiClient.put(path(id), data).then(res => res.data as ServerResult<void>)
  },
  delete(id: string): Promise<ServerResult<void>> {
    return apiClient.delete(path(id)).then(res => res.data as ServerResult<void>)
  },
  restock(id: string, data: StockAdjustmentRequest): Promise<ServerResult<void>> {
    return apiClient.post(path(`${id}/restock`), data).then(res => res.data as ServerResult<void>)
  },
  getLowStock(params?: ServerQueryingParameters): Promise<ServerResult<StockItem[]>> {
    return apiClient.get(path('low-stock'), { params }).then(res => res.data as ServerResult<StockItem[]>)
  },
  getSummary(): Promise<ServerResult<StockSummary[]>> {
    return apiClient.get(path('summary')).then(res => res.data as ServerResult<StockSummary[]>)
  },
  bulkAdjust(data: { items: Array<{ id: string; quantity: number; type: number }> }): Promise<ServerResult<void>> {
    return apiClient.post(path('bulk-adjust'), data).then(res => res.data as ServerResult<void>)
  },
}
