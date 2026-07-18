import { fulfillmentRepository } from '../repositories/fulfillment.repository'
import { orderRepository } from '../../orders/repositories/order.repository'
import type { ServerResult } from '@/shared/api/types/result.types'

export const fulfillmentService = {
  getQueue: fulfillmentRepository.getQueue,
  async markAsShipped(id: string, _trackingNumber?: string): Promise<ServerResult<void>> {
    return orderRepository.complete(id)
  },
}
