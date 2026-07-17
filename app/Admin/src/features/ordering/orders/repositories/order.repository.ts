import apiClient from '@/shared/api/http/api.client'
import { ORDERS } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { OrderListItem, OrderDetail } from '../../types/Order.Response.Type'
import type { CreateOrderRequest, AddOrderItemRequest } from '../../types/Order.Request.Type'

interface OrderLineItem {
  id: string
  orderId: string
  variantId: string
  sku: string
  name: string
  quantity: number
  unitPriceCents: number
  totalPriceCents: number
}

function ordersPath(sub?: string): string {
  return `${ORDERS}/orders${sub ? `/${sub}` : ''}`
}

export const orderRepository = {
  list(params?: ServerQueryingParameters): Promise<ServerResult<OrderListItem[]>> {
    return apiClient.get(ordersPath(), { params }).then(res => res.data as ServerResult<OrderListItem[]>)
  },
  getById(id: string): Promise<ServerResult<OrderDetail>> {
    return apiClient.get(ordersPath(id)).then(res => res.data as ServerResult<OrderDetail>)
  },
  create(data: CreateOrderRequest): Promise<ServerResult<OrderDetail>> {
    return apiClient.post(ordersPath(), data).then(res => res.data as ServerResult<OrderDetail>)
  },
  update(id: string, data: Partial<CreateOrderRequest>): Promise<ServerResult<OrderDetail>> {
    return apiClient.put(ordersPath(id), data).then(res => res.data as ServerResult<OrderDetail>)
  },
  delete(id: string): Promise<ServerResult<void>> {
    return apiClient.delete(ordersPath(id)).then(res => res.data as ServerResult<void>)
  },
  updateStatus(id: string, status: string): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/status`), { status }).then(res => res.data as ServerResult<void>)
  },
  cancel(id: string, reason?: string): Promise<ServerResult<void>> {
    return apiClient.post(ordersPath(`${id}/cancel`), { reason }).then(res => res.data as ServerResult<void>)
  },
  complete(id: string): Promise<ServerResult<void>> {
    return apiClient.post(ordersPath(`${id}/complete`)).then(res => res.data as ServerResult<void>)
  },
  approve(id: string): Promise<ServerResult<void>> {
    return apiClient.post(ordersPath(`${id}/approve`)).then(res => res.data as ServerResult<void>)
  },
  resume(id: string): Promise<ServerResult<void>> {
    return apiClient.post(ordersPath(`${id}/resume`)).then(res => res.data as ServerResult<void>)
  },
  updateShipAddress(id: string, address: Record<string, unknown>): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/ship-address`), address).then(res => res.data as ServerResult<void>)
  },
  updateBillAddress(id: string, address: Record<string, unknown>): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/bill-address`), address).then(res => res.data as ServerResult<void>)
  },
  updateShippingMethod(id: string, shippingMethodId: string): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/shipping-method`), { shippingMethodId }).then(res => res.data as ServerResult<void>)
  },
  listLineItems(id: string): Promise<ServerResult<OrderLineItem[]>> {
    return apiClient.get(ordersPath(`${id}/line-items`)).then(res => res.data as ServerResult<OrderLineItem[]>)
  },
  addLineItem(id: string, data: AddOrderItemRequest): Promise<ServerResult<void>> {
    return apiClient.post(ordersPath(`${id}/line-items`), data).then(res => res.data as ServerResult<void>)
  },
  updateLineItem(id: string, lineItemId: string, data: { quantity?: number }): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/line-items/${lineItemId}`), data).then(res => res.data as ServerResult<void>)
  },
  removeLineItem(id: string, lineItemId: string): Promise<ServerResult<void>> {
    return apiClient.delete(ordersPath(`${id}/line-items/${lineItemId}`)).then(res => res.data as ServerResult<void>)
  },
}
