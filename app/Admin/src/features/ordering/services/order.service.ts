import { orderingRepository } from '../repository/ordering.repository'

export const orderService = {
  list: orderingRepository.orders.list.bind(orderingRepository.orders),
  getById: orderingRepository.orders.getById.bind(orderingRepository.orders),
  create: orderingRepository.orders.create.bind(orderingRepository.orders),
  update: orderingRepository.orders.update.bind(orderingRepository.orders),
  delete: orderingRepository.orders.delete.bind(orderingRepository.orders),
  cancel: orderingRepository.orders.cancel.bind(orderingRepository.orders),
  complete: orderingRepository.orders.complete.bind(orderingRepository.orders),
  approve: orderingRepository.orders.approve.bind(orderingRepository.orders),
  addItem: orderingRepository.orders.addLineItem.bind(orderingRepository.orders),
  removeItem: orderingRepository.orders.removeLineItem.bind(orderingRepository.orders),
  updateStatus: orderingRepository.orders.updateStatus.bind(orderingRepository.orders),
  updateShipAddress: orderingRepository.orders.updateShipAddress.bind(orderingRepository.orders),
  updateBillAddress: orderingRepository.orders.updateBillAddress.bind(orderingRepository.orders),
  createShipment: async (_orderId: string, _data: unknown) => {
    console.warn('createShipment: no backend route exists. See spec/spec-design-admin-api-services.md')
    return { success: false, error: { detail: 'Not implemented — no backend route' } } as const
  },
}
