import { orderRepository } from '../api/order.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { OrderListItemModel, OrderDetailModel } from '../types/order.model.type'
import { mapOrderListItem, mapOrderDetail } from '../mappers/order.mapper'
import type { CreateOrderRequest, AddOrderItemRequest, CancelOrderRequest, UpdateLineItemRequest, UpdateOrderStatusRequest, UpdateShippingMethodRequest, UpdateAddressesRequest } from '../types/order.request.type'

export const orderService = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<OrderListItemModel>> {
    const result = await orderRepository.list(params)
    if (result.isSuccess) {
      return { ...result, items: result.items.map(mapOrderListItem) }
    }
    return result as ServerPagedResult<OrderListItemModel>
  },
  async getById(id: string): Promise<ServerResult<OrderDetailModel>> {
    const result = await orderRepository.getById(id)
    if (result.isSuccess) {
      return { ...result, value: mapOrderDetail(result.value) }
    }
    return result as ServerResult<OrderDetailModel>
  },
  async create(data: CreateOrderRequest): Promise<ServerResult<OrderDetailModel>> {
    const result = await orderRepository.create(data)
    if (result.isSuccess) {
      return { ...result, value: mapOrderDetail(result.value) }
    }
    return result as ServerResult<OrderDetailModel>
  },
  async update(id: string, data: Partial<CreateOrderRequest>): Promise<ServerResult<OrderDetailModel>> {
    const result = await orderRepository.update(id, data)
    if (result.isSuccess) {
      return { ...result, value: mapOrderDetail(result.value) }
    }
    return result as ServerResult<OrderDetailModel>
  },
  delete: orderRepository.delete.bind(orderRepository),
  cancel: orderRepository.cancel.bind(orderRepository),
  complete: orderRepository.complete.bind(orderRepository),
  approve: orderRepository.approve.bind(orderRepository),
  addItem: orderRepository.addLineItem.bind(orderRepository),
  removeItem: orderRepository.removeLineItem.bind(orderRepository),
  updateStatus: orderRepository.updateStatus.bind(orderRepository),
  updateShipAddress: orderRepository.updateShipAddress.bind(orderRepository),
  updateBillAddress: orderRepository.updateBillAddress.bind(orderRepository),
  createShipment: async (_orderId: string, _data: unknown): Promise<ServerResult<void>> => {
    console.warn('createShipment: no backend route exists. See spec/spec-design-admin-api-services.md')
    return { isSuccess: false, statusCode: 501, errors: [], message: 'Not implemented — no backend route', metadata: null, value: undefined as unknown as void }
  },
}
