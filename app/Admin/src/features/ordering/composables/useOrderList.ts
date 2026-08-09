import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { ORDER_FILTER_FIELDS, ORDER_SORT_FIELDS, ORDER_SEARCH_FIELDS } from '../types/order'
import type { OrderListItem } from '../types/order'

export function useOrderList(options?: UsePagedQueryOptions) {
  return usePagedQuery<OrderListItem>(`api/admin/ordering/orders`, {
    allowedFilterFields: ORDER_FILTER_FIELDS,
    allowedSortFields: ORDER_SORT_FIELDS,
    allowedSearchFields: ORDER_SEARCH_FIELDS,
    ...options,
  })
}
