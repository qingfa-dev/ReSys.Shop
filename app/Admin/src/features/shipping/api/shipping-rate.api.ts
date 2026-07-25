import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { ShippingRateResponse, CreateShippingRateRequest, UpdateShippingRateRequest } from '../types'

export class ShippingRateApi {
  static getMany(query: ListQuery): Promise<PagedResult<ShippingRateResponse>> {
    return getPagedList<ShippingRateResponse>('/shipping/shipping-rates', query)
  }
  static async get(id: string): Promise<Result<ShippingRateResponse>> {
    const res = await apiClient.get<Result<ShippingRateResponse>>(`/shipping/shipping-rates/${id}`)
    return res.data
  }
  static async create(data: CreateShippingRateRequest): Promise<Result<ShippingRateResponse>> {
    const res = await apiClient.post<Result<ShippingRateResponse>>('/shipping/shipping-rates', data)
    return res.data
  }
  static async update(id: string, data: UpdateShippingRateRequest): Promise<Result<ShippingRateResponse>> {
    const res = await apiClient.put<Result<ShippingRateResponse>>(`/shipping/shipping-rates/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/shipping/shipping-rates/${id}`)
    return res.data
  }
}
