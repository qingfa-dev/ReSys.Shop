import { get, put, getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { OrderListItem, OrderDetail } from '../types/order'
import { ORDER_FILTER_FIELDS, ORDER_SORT_FIELDS, ORDER_SEARCH_FIELDS } from '../types/order'

export interface OrderTrackingResponse {
  orderId: string
  orderCreatedAt: string
  orderApprovedAt: string | null
  orderCompletedAt: string | null
  orderCanceledAt: string | null
  paymentProcessingAt: string | null
  paymentCompletedAt: string | null
  paymentFailedAt: string | null
  shippedAt: string | null
  deliveredAt: string | null
  deliveryExceptionAt: string | null
  estimatedDeliveryAt: string | null
}

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

export function getOrderTracking(id: string): Promise<Result<OrderTrackingResponse>> {
  return get<Result<OrderTrackingResponse>>(ENDPOINTS.orderTracking(id))
}

// Backend route is PUT api/storefront/orders/{id}/cancel (returns a value-less Result).
export function cancelOrder(id: string): Promise<Result<null>> {
  return put<Result<null>>(ENDPOINTS.orderCancel(id))
}
