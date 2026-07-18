import { orderRepository } from '../orders/repositories/order.repository'
import type { ServerResult } from '@/shared/api/types/result.types'

export const orderService = {
  list: orderRepository.list.bind(orderRepository),
  getById: orderRepository.getById.bind(orderRepository),
  create: orderRepository.create.bind(orderRepository),
  update: orderRepository.update.bind(orderRepository),
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
    return { isSuccess: false, statusCode: 501, errors: [], message: 'Not implemented — no backend route', metadata: null, value: undefined }
  },
}
