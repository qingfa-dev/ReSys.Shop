import { fulfillmentRepository } from '../repositories/fulfillment.repository'
import { orderRepository } from '../../orders/repositories/order.repository'

export const fulfillmentService = {
  getQueue: fulfillmentRepository.getQueue,
  async markAsShipped(id: string, _trackingNumber?: string): Promise<any> {
    return orderRepository.complete(id)
  },
}
