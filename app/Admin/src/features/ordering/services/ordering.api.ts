import apiClient from '@/shared/api/http/api.client'
import { ORDERS } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { OrderListItem, OrderDetail, CreateOrderRequest, AddOrderItemRequest, CancelOrderRequest } from '../types/order.types'

export const orderingApi = {
  orders: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<OrderListItem[]>> {
      return apiClient.get(`${ORDERS}/orders`, { params })
    },
    async getById(id: string): Promise<ApiResult<OrderDetail>> {
      return apiClient.get(`${ORDERS}/orders/${id}`)
    },
    async create(data: CreateOrderRequest): Promise<ApiResult<OrderDetail>> {
      return apiClient.post(`${ORDERS}/orders`, data)
    },
    async update(id: string, data: Partial<CreateOrderRequest>): Promise<ApiResult<OrderDetail>> {
      return apiClient.put(`${ORDERS}/orders/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${ORDERS}/orders/${id}`)
    },
    // State transitions
    async updateStatus(id: string, status: string): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/status`, { status })
    },
    async cancel(id: string, reason?: string): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/cancel`, { reason } as CancelOrderRequest)
    },
    async complete(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/complete`)
    },
    async approve(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/approve`)
    },
    async resume(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/resume`)
    },
    // Addresses
    async updateShipAddress(id: string, address: Record<string, unknown>): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/ship-address`, address)
    },
    async updateBillAddress(id: string, address: Record<string, unknown>): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/bill-address`, address)
    },
    async updateShippingMethod(id: string, shippingMethodId: string): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/shipping-method`, { shippingMethodId })
    },
    // Line items
    async listLineItems(id: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${ORDERS}/orders/${id}/line-items`)
    },
    async addLineItem(id: string, data: AddOrderItemRequest): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/line-items`, data)
    },
    async updateLineItem(id: string, lineItemId: string, data: { quantity?: number }): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/line-items/${lineItemId}`, data)
    },
    async removeLineItem(id: string, lineItemId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${ORDERS}/orders/${id}/line-items/${lineItemId}`)
    },
  },

  fulfillments: {
    async getQueue(params?: ServerQueryingParameters): Promise<ApiResult<OrderListItem[]>> {
      return apiClient.get(`${ORDERS}/orders`, { params: { ...params, state: 'Processing' } })
    },
  },
}
