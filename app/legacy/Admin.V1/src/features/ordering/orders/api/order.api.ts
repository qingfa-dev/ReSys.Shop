import apiClient from '@/common/api/http/api.client'
import { ORDERS } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { OrderListItem, OrderDetail } from '../types/order.response'
import type { CreateOrderRequest, AddOrderItemRequest, CancelOrderRequest, UpdateLineItemRequest, UpdateOrderStatusRequest, UpdateShippingMethodRequest, UpdateAddressesRequest } from '../types/order.request'
import type { OrderListItemModel, OrderDetailModel } from '../types/order.model'
import { mapOrderListItem, mapOrderDetail } from '../models/order.mapper'

export interface OrderLineItem {
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
  createShipment: async (_orderId: string, _data: unknown): Promise<ServerResult<void>> => ({ isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined as unknown as void }),
  refund: async (_orderId: string, _paymentId: string, _data: unknown): Promise<ServerResult<void>> => ({ isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined as unknown as void }),
  cancelShipment: async (_orderId: string, _shipmentId: string): Promise<ServerResult<void>> => ({ isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined as unknown as void }),
}
