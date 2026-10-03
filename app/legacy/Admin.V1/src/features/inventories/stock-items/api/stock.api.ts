import apiClient from '@/common/api/http/api.client'
import { INVENTORY } from '@/common/api/constants'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { StockItem, StockItemDetail, StockSummary } from '../types/stock-item.response'
import type { StockAdjustmentRequest, CreateStockItemRequest, UpdateStockItemRequest, BulkAdjustRequest } from '../types/stock-item.request'
import type { StockItemQuery } from '../types/stock-item.query'
import { mapStockItem, mapStockItemDetail } from '../models/stock-item.mapper'

function path(sub?: string): string {
  return `${INVENTORY}/stock-items${sub ? `/${sub}` : ''}`
}

export const stockRepository = {
  list(params: StockItemQuery): Promise<ServerPagedResult<StockItem>> {
    return apiClient.get(path(), { params }).then(res => {
      const result = res.data as ServerPagedResult<StockItem>
      return { ...result, items: result.items.map(mapStockItem) }
    })
  },
  getById(id: string): Promise<ServerResult<StockItemDetail>> {
    return apiClient.get(path(id)).then(res => {
      const result = res.data as ServerResult<StockItemDetail>
      return { ...result, value: mapStockItemDetail(result.value) }
    })
  },
  create(data: CreateStockItemRequest): Promise<ServerResult<StockItemDetail>> {
    return apiClient.post(path(), data).then(res => {
      const result = res.data as ServerResult<StockItemDetail>
      return { ...result, value: mapStockItemDetail(result.value) }
    })
  },
  update(id: string, data: UpdateStockItemRequest): Promise<ServerResult<void>> {
    return apiClient.put(path(id), data).then(res => res.data as ServerResult<void>)
  },
  delete(id: string): Promise<ServerResult<void>> {
    return apiClient.delete(path(id)).then(res => res.data as ServerResult<void>)
  },
  restock(id: string, data: StockAdjustmentRequest): Promise<ServerResult<void>> {
    return apiClient.post(path(`${id}/restock`), data).then(res => res.data as ServerResult<void>)
  },
  getLowStock(params?: ServerQueryingParameters): Promise<ServerResult<StockItem[]>> {
    return apiClient.get(path('low-stock'), { params }).then(res => {
      const result = res.data as ServerResult<StockItem[]>
      return { ...result, value: result.value.map(mapStockItem) }
    })
  },
  getSummary(): Promise<ServerResult<StockSummary[]>> {
    return apiClient.get(path('summary')).then(res => res.data as ServerResult<StockSummary[]>)
  },
  bulkAdjust(data: BulkAdjustRequest): Promise<ServerResult<void>> {
    return apiClient.post(path('bulk-adjust'), data).then(res => res.data as ServerResult<void>)
  },
  importStockItems(file: File): Promise<ServerResult<void>> {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient
      .post(path('import'), formData, { headers: { 'Content-Type': 'multipart/form-data' } })
      .then(res => res.data as ServerResult<void>)
  },
}
