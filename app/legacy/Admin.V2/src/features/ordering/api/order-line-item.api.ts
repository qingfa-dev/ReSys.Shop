import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { OrderLineItemResponse, AddLineItemRequest, UpdateLineItemRequest } from '../types'

export class OrderLineItemApi {
  static async getMany(orderId: string): Promise<Result<OrderLineItemResponse[]>> {
    const res = await apiClient.get<Result<OrderLineItemResponse[]>>(`/ordering/orders/${orderId}/line-items`)
    return res.data
  }
  static async get(orderId: string, lineItemId: string): Promise<Result<OrderLineItemResponse>> {
    const res = await apiClient.get<Result<OrderLineItemResponse>>(`/ordering/orders/${orderId}/line-items/${lineItemId}`)
    return res.data
  }
  static async create(orderId: string, data: AddLineItemRequest): Promise<Result<OrderLineItemResponse>> {
    const res = await apiClient.post<Result<OrderLineItemResponse>>(`/ordering/orders/${orderId}/line-items`, data)
    return res.data
  }
  static async update(orderId: string, lineItemId: string, data: UpdateLineItemRequest): Promise<Result<OrderLineItemResponse>> {
    const res = await apiClient.put<Result<OrderLineItemResponse>>(`/ordering/orders/${orderId}/line-items/${lineItemId}`, data)
    return res.data
  }
  static async delete(orderId: string, lineItemId: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/ordering/orders/${orderId}/line-items/${lineItemId}`)
    return res.data
  }
}
