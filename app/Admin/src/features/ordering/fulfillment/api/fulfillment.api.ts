import apiClient from '@/shared/api/http/api.client'
import { ORDERS } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { OrderListItem } from '../../types/Order.Response.Type'

function fulfillmentPath(): string {
  return `${ORDERS}/orders`
}

export const fulfillmentRepository = {
  getQueue(params?: ServerQueryingParameters): Promise<ServerResult<OrderListItem[]>> {
    return apiClient.get(fulfillmentPath(), { params: { ...params, state: 'Processing' } }).then(res => res.data as ServerResult<OrderListItem[]>)
  },
}
