import { orderingApi } from '../../services/ordering.api'

export const fulfillmentService = {
  getQueue: orderingApi.fulfillments.getQueue,
  async markAsShipped(id: string, _trackingNumber?: string): Promise<any> {
    return orderingApi.orders.complete(id)
  },
}
