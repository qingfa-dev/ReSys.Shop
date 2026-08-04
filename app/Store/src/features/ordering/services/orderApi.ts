import { get, put, getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { OrderListItem, OrderDetail } from '../types/order'
import { ORDER_FILTER_FIELDS, ORDER_SORT_FIELDS, ORDER_SEARCH_FIELDS } from '../types/order'

export function getOrders(params: QueryingParameters): Promise<PagedResult<OrderListItem>> {
  return getPaged<OrderListItem>(ENDPOINTS.orders, params, {
    allowedFilterFields: ORDER_FILTER_FIELDS,
    allowedSortFields: ORDER_SORT_FIELDS,
    allowedSearchFields: ORDER_SEARCH_FIELDS,
  })
}

export function getOrder(id: string): Promise<Result<OrderDetail>> {
  return get<Result<OrderDetail>>(ENDPOINTS.orderById(id))
}

// Backend route is PUT api/storefront/orders/{id}/cancel (returns a value-less Result).
export function cancelOrder(id: string): Promise<Result<null>> {
  return put<Result<null>>(ENDPOINTS.orderCancel(id))
}
