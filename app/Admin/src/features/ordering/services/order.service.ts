import { orderingApi } from './ordering.api'

export const orderService = {
  list: orderingApi.orders.list,
  getById: orderingApi.orders.getById,
  create: orderingApi.orders.create,
  update: orderingApi.orders.update,
  delete: orderingApi.orders.delete,
  cancel: orderingApi.orders.cancel,
  complete: orderingApi.orders.complete,
  approve: orderingApi.orders.approve,
  addItem: orderingApi.orders.addLineItem,
  removeItem: orderingApi.orders.removeLineItem,
  updateStatus: orderingApi.orders.updateStatus,
  updateShipAddress: orderingApi.orders.updateShipAddress,
  updateBillAddress: orderingApi.orders.updateBillAddress,
}
