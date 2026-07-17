import { orderingRepository } from '../../repository/ordering.repository'

export const fulfillmentService = {
  getQueue: orderingRepository.fulfillments.getQueue,
  async markAsShipped(id: string, _trackingNumber?: string): Promise<any> {
    return orderingRepository.orders.complete(id)
  },
}
