import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { ShippingMethodResponse, CreateShippingMethodRequest, UpdateShippingMethodRequest } from '../types'

export class ShippingMethodApi {
  static getMany(query: ListQuery): Promise<PagedResult<ShippingMethodResponse>> {
    return getPagedList<ShippingMethodResponse>('/shipping/shipping-methods', query)
  }
  static async get(id: string): Promise<Result<ShippingMethodResponse>> {
    const res = await apiClient.get<Result<ShippingMethodResponse>>(`/shipping/shipping-methods/${id}`)
    return res.data
  }
  static async create(data: CreateShippingMethodRequest): Promise<Result<ShippingMethodResponse>> {
    const res = await apiClient.post<Result<ShippingMethodResponse>>('/shipping/shipping-methods', data)
    return res.data
  }
  static async update(id: string, data: UpdateShippingMethodRequest): Promise<Result<ShippingMethodResponse>> {
    const res = await apiClient.put<Result<ShippingMethodResponse>>(`/shipping/shipping-methods/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/shipping/shipping-methods/${id}`)
    return res.data
  }
  static async activate(id: string): Promise<Result<void>> {
    const res = await apiClient.patch<Result<void>>(`/shipping/shipping-methods/${id}/activate`)
    return res.data
  }
  static async deactivate(id: string): Promise<Result<void>> {
    const res = await apiClient.patch<Result<void>>(`/shipping/shipping-methods/${id}/deactivate`)
    return res.data
  }
}
