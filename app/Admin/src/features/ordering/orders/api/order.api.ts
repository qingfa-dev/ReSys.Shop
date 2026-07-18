import apiClient from '@/shared/api/http/api.client'
import { ORDERS } from '@/shared/api/constants'
import type { ServerPagedResult, ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { OrderListItem, OrderDetail } from '../types/order.response.type'
import type { CreateOrderRequest, AddOrderItemRequest, CancelOrderRequest, UpdateLineItemRequest, UpdateOrderStatusRequest, UpdateShippingMethodRequest, UpdateAddressesRequest } from '../types/order.request.type'
import type { OrderListItemModel, OrderDetailModel } from '../types/order.model.type'
import { mapOrderListItem, mapOrderDetail } from '../mappers/order.mapper'

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
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<OrderListItemModel>> {
    const result = await apiClient.get(ordersPath(), { params }).then(res => res.data as ServerPagedResult<OrderListItem>)
    if (result.isSuccess) {
      return { ...result, items: result.items.map(mapOrderListItem) }
    }
    return result as ServerPagedResult<OrderListItemModel>
  },
  async getById(id: string): Promise<ServerResult<OrderDetailModel>> {
    const result = await apiClient.get(ordersPath(id)).then(res => res.data as ServerResult<OrderDetail>)
    if (result.isSuccess) {
      return { ...result, value: mapOrderDetail(result.value) }
    }
    return result as ServerResult<OrderDetailModel>
  },
  async create(data: CreateOrderRequest): Promise<ServerResult<OrderDetailModel>> {
    const result = await apiClient.post(ordersPath(), data).then(res => res.data as ServerResult<OrderDetail>)
    if (result.isSuccess) {
      return { ...result, value: mapOrderDetail(result.value) }
    }
    return result as ServerResult<OrderDetailModel>
  },
  async update(id: string, data: Partial<CreateOrderRequest>): Promise<ServerResult<OrderDetailModel>> {
    const result = await apiClient.put(ordersPath(id), data).then(res => res.data as ServerResult<OrderDetail>)
    if (result.isSuccess) {
      return { ...result, value: mapOrderDetail(result.value) }
    }
    return result as ServerResult<OrderDetailModel>
  },
  delete(id: string): Promise<ServerResult<void>> {
    return apiClient.delete(ordersPath(id)).then(res => res.data as ServerResult<void>)
  },
  updateStatus(id: string, data: UpdateOrderStatusRequest): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/status`), data).then(res => res.data as ServerResult<void>)
  },
  cancel(id: string, data?: CancelOrderRequest): Promise<ServerResult<void>> {
    return apiClient.post(ordersPath(`${id}/cancel`), data).then(res => res.data as ServerResult<void>)
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
  updateShipAddress(id: string, address: UpdateAddressesRequest['shippingAddress']): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/ship-address`), address).then(res => res.data as ServerResult<void>)
  },
  updateBillAddress(id: string, address: UpdateAddressesRequest['billingAddress']): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/bill-address`), address).then(res => res.data as ServerResult<void>)
  },
  updateShippingMethod(id: string, data: UpdateShippingMethodRequest): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/shipping-method`), data).then(res => res.data as ServerResult<void>)
  },
  listLineItems(id: string): Promise<ServerPagedResult<OrderLineItem>> {
    return apiClient.get(ordersPath(`${id}/line-items`)).then(res => res.data as ServerPagedResult<OrderLineItem>)
  },
  getLineItemById(id: string, lineItemId: string): Promise<ServerResult<OrderLineItem>> {
    return apiClient.get(ordersPath(`${id}/line-items/${lineItemId}`)).then(res => res.data as ServerResult<OrderLineItem>)
  },
  addLineItem(id: string, data: AddOrderItemRequest): Promise<ServerResult<void>> {
    return apiClient.post(ordersPath(`${id}/line-items`), data).then(res => res.data as ServerResult<void>)
  },
  updateLineItem(id: string, lineItemId: string, data: UpdateLineItemRequest): Promise<ServerResult<void>> {
    return apiClient.put(ordersPath(`${id}/line-items/${lineItemId}`), data).then(res => res.data as ServerResult<void>)
  },
  removeLineItem(id: string, lineItemId: string): Promise<ServerResult<void>> {
    return apiClient.delete(ordersPath(`${id}/line-items/${lineItemId}`)).then(res => res.data as ServerResult<void>)
  },
}
