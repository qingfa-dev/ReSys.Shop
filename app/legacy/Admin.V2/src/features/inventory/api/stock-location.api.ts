import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { StockLocationResponse, CreateStockLocationRequest, UpdateStockLocationRequest } from '../types'

export class StockLocationApi {
  static getMany(query: ListQuery): Promise<PagedResult<StockLocationResponse>> {
    return getPagedList<StockLocationResponse>('/inventory/stock-locations', query)
  }
  static async get(id: string): Promise<Result<StockLocationResponse>> {
    const res = await apiClient.get<Result<StockLocationResponse>>(`/inventory/stock-locations/${id}`)
    return res.data
  }
  static async create(data: CreateStockLocationRequest): Promise<Result<StockLocationResponse>> {
    const res = await apiClient.post<Result<StockLocationResponse>>('/inventory/stock-locations', data)
    return res.data
  }
  static async update(id: string, data: UpdateStockLocationRequest): Promise<Result<StockLocationResponse>> {
    const res = await apiClient.put<Result<StockLocationResponse>>(`/inventory/stock-locations/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/inventory/stock-locations/${id}`)
    return res.data
  }
  static async setDefault(id: string): Promise<Result<void>> {
    const res = await apiClient.put<Result<void>>(`/inventory/stock-locations/${id}/default`)
    return res.data
  }
}
