import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { StockMovementResponse } from '../types'

export class StockMovementApi {
  static getMany(query: ListQuery): Promise<PagedResult<StockMovementResponse>> {
    return getPagedList<StockMovementResponse>('/inventory/stock-movements', query)
  }
  static async get(id: string): Promise<Result<StockMovementResponse>> {
    const res = await apiClient.get<Result<StockMovementResponse>>(`/inventory/stock-movements/${id}`)
    return res.data
  }
}
