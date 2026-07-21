import apiClient from '@/common/api/http/api.client'
import { ORDERS } from '@/common/api/constants'
import type { ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { OrderListItem } from '../../orders/types/order.response'
import type { OrderListItemModel } from '../../orders/types/order.model'
import { mapOrderListItem } from '../../orders/models/order.mapper'
import { orderRepository } from '../../orders/api/order.api'

function fulfillmentPath(): string {
  return `${ORDERS}/orders`
}

export const fulfillmentRepository = {
  async getQueue(params?: ServerQueryingParameters): Promise<ServerResult<OrderListItemModel[]>> {
    const result = await apiClient.get(fulfillmentPath(), { params: { ...params, state: 'Processing' } }).then(res => res.data as ServerResult<OrderListItem[]>)
    if (result.isSuccess) {
      return { ...result, value: result.value.map(mapOrderListItem) }
    }
    return result as ServerResult<OrderListItemModel[]>
  },

  markAsShipped(id: string, _trackingNumber?: string): Promise<ServerResult<void>> {
    return orderRepository.complete(id)
  },
}
