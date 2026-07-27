import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { StockReservationResponse } from '../types'

export class StockReservationApi {
  static getMany(query: ListQuery): Promise<PagedResult<StockReservationResponse>> {
    return getPagedList<StockReservationResponse>('/inventory/stock-reservations', query)
  }
  static async get(id: string): Promise<Result<StockReservationResponse>> {
    const res = await apiClient.get<Result<StockReservationResponse>>(`/inventory/stock-reservations/${id}`)
    return res.data
  }
  static async cancel(id: string): Promise<Result<void>> {
    const res = await apiClient.post<Result<void>>(`/inventory/stock-reservations/${id}/cancel`)
    return res.data
  }
}
