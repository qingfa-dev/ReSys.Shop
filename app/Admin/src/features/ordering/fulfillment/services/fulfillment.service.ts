import { fulfillmentRepository } from '../api/fulfillment.api'
import { orderRepository } from '../../orders/api/order.api'
import type { ServerResult } from '@/shared/api/types/result.types'

export const fulfillmentService = {
  getQueue: fulfillmentRepository.getQueue,
  async markAsShipped(id: string, _trackingNumber?: string): Promise<ServerResult<void>> {
    return orderRepository.complete(id)
  },
}
