import { orderingApi } from './ordering.api'

export const orderService = {
  list: orderingApi.orders.list,
  getById: orderingApi.orders.getById,
  create: orderingApi.orders.create,
  update: orderingApi.orders.update,
  delete: orderingApi.orders.delete,
  createShipment: orderingApi.orders.createShipment,
  cancelShipment: orderingApi.orders.cancelShipment,
  addItem: orderingApi.orders.addItem,
  updateAddresses: orderingApi.orders.updateAddresses,
  updateState: orderingApi.orders.updateState,
  cancelOrder: orderingApi.orders.cancelOrder,
  refundPayment: orderingApi.orders.refundPayment,
}