import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { StockTransferResponse, CreateStockTransferRequest } from '../types'

export class StockTransferApi {
  static getMany(query: ListQuery): Promise<PagedResult<StockTransferResponse>> {
    return getPagedList<StockTransferResponse>('/inventory/stock-transfers', query)
  }
  static async get(id: string): Promise<Result<StockTransferResponse>> {
    const res = await apiClient.get<Result<StockTransferResponse>>(`/inventory/stock-transfers/${id}`)
    return res.data
  }
  static async create(data: CreateStockTransferRequest): Promise<Result<StockTransferResponse>> {
    const res = await apiClient.post<Result<StockTransferResponse>>('/inventory/stock-transfers', data)
    return res.data
  }
  static async transfer(id: string): Promise<Result<StockTransferResponse>> {
    const res = await apiClient.post<Result<StockTransferResponse>>(`/inventory/stock-transfers/${id}/transfer`)
    return res.data
  }
  static async receive(id: string): Promise<Result<StockTransferResponse>> {
    const res = await apiClient.post<Result<StockTransferResponse>>(`/inventory/stock-transfers/${id}/receive`)
    return res.data
  }
  static async cancel(id: string): Promise<Result<StockTransferResponse>> {
    const res = await apiClient.post<Result<StockTransferResponse>>(`/inventory/stock-transfers/${id}/cancel`)
    return res.data
  }
}
