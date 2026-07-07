import { createModuleApi, apiClient } from '@/shared/api'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { OrderListItem, OrderDetail, CreateOrderRequest, AddOrderItemRequest, UpdateAddressesRequest, CancelOrderRequest, CreateShipmentRequest, RefundPaymentRequest } from '../types/order.types'
import { ORDERS } from '@/shared/api/constants'

export const orderingApi = {
  orders: {
    ...createModuleApi<OrderDetail, CreateOrderRequest>({ basePath: ORDERS }),

    async list(params?: ServerQueryingParameters): Promise<ApiResult<OrderListItem[]>> {
      return apiClient.get(ORDERS, { params })
    },
    async createShipment(orderId: string, data: CreateShipmentRequest): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/${orderId}/shipments`, data)
    },
    async cancelShipment(orderId: string, shipmentId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${ORDERS}/${orderId}/shipments/${shipmentId}`)
    },
    async addItem(id: string, data: AddOrderItemRequest): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/${id}/items`, data)
    },
    async updateAddresses(id: string, data: UpdateAddressesRequest): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/${id}/addresses`, data)
    },
    async updateState(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/${id}/advance`)
    },
    async cancelOrder(id: string, reason?: string): Promise<ApiResult<void>> {
      const data: CancelOrderRequest = { reason }
      return apiClient.post(`${ORDERS}/${id}/cancel`, data)
    },
    async refundPayment(orderId: string, paymentId: string, data: RefundPaymentRequest): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/${orderId}/payments/${paymentId}/refund`, data)
    },
  },

  fulfillments: {
    async getQueue(params?: ServerQueryingParameters): Promise<ApiResult<OrderListItem[]>> {
      return apiClient.get(ORDERS, { params: { ...params, state: 'Processing' } })
    },
  },
}
