import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import type { OrderListItem } from '../types/order'
import { OrderApi } from '../services/orderApi'

export function useOrderList(options?: UsePagedQueryOptions) {
  return usePagedQuery<OrderListItem>((params) => OrderApi.getOrders(params), { ...options })
}
