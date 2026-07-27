import apiClient from '@/shared/api/client'
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { Result, PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { OrderResponse, CreateOrderRequest, UpdateOrderStatusRequest, UpdateAddressRequest } from '../types'

export class OrderApi {
  static getMany(query: ListQuery): Promise<PagedResult<OrderResponse>> {
    return getPagedList<OrderResponse>('/ordering/orders', query)
  }
  static async get(id: string): Promise<Result<OrderResponse>> {
    const res = await apiClient.get<Result<OrderResponse>>(`/ordering/orders/${id}`)
    return res.data
  }
  static async create(data: CreateOrderRequest): Promise<Result<OrderResponse>> {
    const res = await apiClient.post<Result<OrderResponse>>('/ordering/orders', data)
    return res.data
  }
  static async update(id: string, data: { notes?: string | null }): Promise<Result<OrderResponse>> {
    const res = await apiClient.put<Result<OrderResponse>>(`/ordering/orders/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/ordering/orders/${id}`)
    return res.data
  }
  static async cancel(id: string): Promise<Result<OrderResponse>> {
    const res = await apiClient.post<Result<OrderResponse>>(`/ordering/orders/${id}/cancel`)
    return res.data
  }
  static async complete(id: string): Promise<Result<OrderResponse>> {
    const res = await apiClient.post<Result<OrderResponse>>(`/ordering/orders/${id}/complete`)
    return res.data
  }
  static async approve(id: string): Promise<Result<OrderResponse>> {
    const res = await apiClient.post<Result<OrderResponse>>(`/ordering/orders/${id}/approve`)
    return res.data
  }
  static async resume(id: string): Promise<Result<OrderResponse>> {
    const res = await apiClient.post<Result<OrderResponse>>(`/ordering/orders/${id}/resume`)
    return res.data
  }
  static async updateStatus(id: string, data: UpdateOrderStatusRequest): Promise<Result<OrderResponse>> {
    const res = await apiClient.put<Result<OrderResponse>>(`/ordering/orders/${id}/status`, data)
    return res.data
  }
  static async updateShipAddress(id: string, data: UpdateAddressRequest): Promise<Result<OrderResponse>> {
    const res = await apiClient.put<Result<OrderResponse>>(`/ordering/orders/${id}/ship-address`, data)
    return res.data
  }
  static async updateBillAddress(id: string, data: UpdateAddressRequest): Promise<Result<OrderResponse>> {
    const res = await apiClient.put<Result<OrderResponse>>(`/ordering/orders/${id}/bill-address`, data)
    return res.data
  }
  static async updateShippingMethod(id: string, data: { shippingMethod: string }): Promise<Result<OrderResponse>> {
    const res = await apiClient.put<Result<OrderResponse>>(`/ordering/orders/${id}/shipping-method`, data)
    return res.data
  }
}
