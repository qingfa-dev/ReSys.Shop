import { orderingApi } from '../../services/ordering.api'

export const fulfillmentService = {
  getQueue: orderingApi.fulfillments.getQueue,
  markAsShipped: orderingApi.orders.complete,
}
