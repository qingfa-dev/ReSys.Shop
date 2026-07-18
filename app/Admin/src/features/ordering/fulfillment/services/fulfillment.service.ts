import { fulfillmentRepository } from '../api/fulfillment.api'
import { orderRepository } from '../../orders/api/order.api'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { OrderListItemModel } from '../../orders/types/order.model.type'
import { mapOrderListItem } from '../../orders/mappers/order.mapper'

export const fulfillmentService = {
  async getQueue(params?: ServerQueryingParameters): Promise<ServerResult<OrderListItemModel[]>> {
    const result = await fulfillmentRepository.getQueue(params)
    if (result.isSuccess) {
      return { ...result, value: result.value.map(mapOrderListItem) }
    }
    return result as ServerResult<OrderListItemModel[]>
  },
  async markAsShipped(id: string, _trackingNumber?: string): Promise<ServerResult<void>> {
    return orderRepository.complete(id)
  },
}
