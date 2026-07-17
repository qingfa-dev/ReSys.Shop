import apiClient from '@/shared/api/http/api.client'
import { ORDERS } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { OrderListItem, OrderDetail, CreateOrderRequest, AddOrderItemRequest } from '../types/order.types'

function ordersPath(sub?: string): string {
  return `${ORDERS}/orders${sub ? `/${sub}` : ''}`
}

export const orderingRepository = {
  orders: {
    list(params?: ServerQueryingParameters): Promise<ApiResult<OrderListItem[]>> {
      return apiClient.get(ordersPath(), { params })
    },
    getById(id: string): Promise<ApiResult<OrderDetail>> {
      return apiClient.get(ordersPath(id))
    },
    create(data: CreateOrderRequest): Promise<ApiResult<OrderDetail>> {
      return apiClient.post(ordersPath(), data)
    },
    update(id: string, data: Partial<CreateOrderRequest>): Promise<ApiResult<OrderDetail>> {
      return apiClient.put(ordersPath(id), data)
    },
    delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(ordersPath(id))
    },
    updateStatus(id: string, status: string): Promise<ApiResult<void>> {
      return apiClient.put(ordersPath(`${id}/status`), { status })
    },
    cancel(id: string, reason?: string): Promise<ApiResult<void>> {
      return apiClient.post(ordersPath(`${id}/cancel`), { reason })
    },
    complete(id: string): Promise<ApiResult<void>> {
      return apiClient.post(ordersPath(`${id}/complete`))
    },
    approve(id: string): Promise<ApiResult<void>> {
      return apiClient.post(ordersPath(`${id}/approve`))
    },
    resume(id: string): Promise<ApiResult<void>> {
      return apiClient.post(ordersPath(`${id}/resume`))
    },
    updateShipAddress(id: string, address: Record<string, unknown>): Promise<ApiResult<void>> {
      return apiClient.put(ordersPath(`${id}/ship-address`), address)
    },
    updateBillAddress(id: string, address: Record<string, unknown>): Promise<ApiResult<void>> {
      return apiClient.put(ordersPath(`${id}/bill-address`), address)
    },
    updateShippingMethod(id: string, shippingMethodId: string): Promise<ApiResult<void>> {
      return apiClient.put(ordersPath(`${id}/shipping-method`), { shippingMethodId })
    },
    listLineItems(id: string): Promise<ApiResult<any[]>> {
      return apiClient.get(ordersPath(`${id}/line-items`))
    },
    addLineItem(id: string, data: AddOrderItemRequest): Promise<ApiResult<void>> {
      return apiClient.post(ordersPath(`${id}/line-items`), data)
    },
    updateLineItem(id: string, lineItemId: string, data: { quantity?: number }): Promise<ApiResult<void>> {
      return apiClient.put(ordersPath(`${id}/line-items/${lineItemId}`), data)
    },
    removeLineItem(id: string, lineItemId: string): Promise<ApiResult<void>> {
      return apiClient.delete(ordersPath(`${id}/line-items/${lineItemId}`))
    },
  },

  fulfillments: {
    getQueue(params?: ServerQueryingParameters): Promise<ApiResult<OrderListItem[]>> {
      return apiClient.get(ordersPath(), { params: { ...params, state: 'Processing' } })
    },
  },
}
