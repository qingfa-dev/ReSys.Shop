import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { StockItemResponse, StockSummaryResponse, CreateStockItemRequest, UpdateStockItemRequest, BulkAdjustRequest, RestockRequest } from '../types'

export class StockItemApi {
  static getMany(query: ListQuery): Promise<PagedResult<StockItemResponse>> {
    return getPagedList<StockItemResponse>('/inventory/stock-items', query)
  }
  static async get(id: string): Promise<Result<StockItemResponse>> {
    const res = await apiClient.get<Result<StockItemResponse>>(`/inventory/stock-items/${id}`)
    return res.data
  }
  static async getLowStock(): Promise<Result<StockItemResponse[]>> {
    const res = await apiClient.get<Result<StockItemResponse[]>>('/inventory/stock-items/low-stock')
    return res.data
  }
  static async getSummary(): Promise<Result<StockSummaryResponse>> {
    const res = await apiClient.get<Result<StockSummaryResponse>>('/inventory/stock-items/summary')
    return res.data
  }
  static async create(data: CreateStockItemRequest): Promise<Result<StockItemResponse>> {
    const res = await apiClient.post<Result<StockItemResponse>>('/inventory/stock-items', data)
    return res.data
  }
  static async bulkAdjust(data: BulkAdjustRequest): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>('/inventory/stock-items/bulk-adjust', data)
    return res.data
  }
  static async importFile(formData: FormData): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>('/inventory/stock-items/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return res.data
  }
  static async restock(id: string, data: RestockRequest): Promise<Result<StockItemResponse>> {
    const res = await apiClient.post<Result<StockItemResponse>>(`/inventory/stock-items/${id}/restock`, data)
    return res.data
  }
  static async update(id: string, data: UpdateStockItemRequest): Promise<Result<StockItemResponse>> {
    const res = await apiClient.put<Result<StockItemResponse>>(`/inventory/stock-items/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/inventory/stock-items/${id}`)
    return res.data
  }
}
