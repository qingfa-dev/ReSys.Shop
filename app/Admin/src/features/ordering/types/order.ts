import type { QueryingParameters } from '@/shared/types/querying'

export type OrderStatus = 'Draft' | 'Placed' | 'Canceled' | 'Expired'

export type CheckoutState = 'Address' | 'Delivery' | 'Payment' | 'Confirm' | 'Complete'

export interface OrderRequest {
  currency: string
  email?: string
  specialInstructions?: string
  billAddressId?: string
  shipAddressId?: string
  shippingMethodId?: string
}

export interface OrderListItem extends OrderRequest {
  id: string
  number: string
  status: OrderStatus
  total: number
  paymentTotal: number
  paymentState?: string
  shipmentState?: string
  createdAtUtc: string
  completedAtUtc?: string
}

export interface OrderDetail extends OrderRequest {
  id: string
  number: string
  status: OrderStatus
  checkoutState: CheckoutState
  itemTotal: number
  adjustmentTotal: number
  shipmentTotal: number
  total: number
  paymentTotal: number
  outstandingBalance: number
  paymentState?: string
  shipmentState?: string
  userId?: string
  storeId?: string
  itemCount: number
  approvedById?: string
  approvedAtUtc?: string
  completedAtUtc?: string
  canceledAtUtc?: string
  createdAtUtc: string
  modifiedAtUtc?: string
}

export interface LineItem {
  id: string
  variantId: string
  quantity: number
  price: number
  total: number
  adjustmentTotal: number
  currency: string
  createdAtUtc: string
}

export interface OrderQuery {
  status?: OrderStatus
  checkoutState?: CheckoutState
  currency?: string
  storeId?: string
  search?: string
  sortBy?: 'number' | 'total' | 'completedAtUtc' | 'createdAtUtc' | 'status'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const ORDER_FILTER_FIELDS = [
  'status',
  'checkoutState',
  'currency',
  'userId',
  'storeId',
  'isDeleted',
]

export const ORDER_SORT_FIELDS = ['number', 'total', 'completedAtUtc', 'createdAtUtc', 'status']

export const ORDER_SEARCH_FIELDS = ['number', 'email']

export function toOrderQueryParams(query: OrderQuery): QueryingParameters {
  const filters: string[] = []

  if (query.status !== undefined) {
    filters.push(`status=${query.status}`)
  }
  if (query.checkoutState !== undefined) {
    filters.push(`checkoutState=${query.checkoutState}`)
  }
  if (query.currency !== undefined && query.currency !== '') {
    filters.push(`currency=${query.currency}`)
  }
  if (query.storeId !== undefined && query.storeId !== '') {
    filters.push(`storeId=${query.storeId}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    searchFields: ORDER_SEARCH_FIELDS,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}

export interface AddLineItemRequest {
  variantId: string
  quantity: number
  price: number
}

export interface UpdateLineItemRequest {
  quantity: number
}

export interface CancelOrderRequest {
  reason?: string
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus
}

export interface UpdateOrderAddressRequest {
  addressId: string
}

export interface UpdateOrderShippingMethodRequest {
  shippingMethodId: string
}
