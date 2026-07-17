import { fulfillmentRepository } from '../../repository/fulfillment.repository'
import { orderRepository } from '../../repository/order.repository'

export const fulfillmentService = {
  getQueue: fulfillmentRepository.getQueue,
  async markAsShipped(id: string, _trackingNumber?: string): Promise<any> {
    return orderRepository.complete(id)
  },
}
