import type { QueryingParameters } from '@/shared/types/querying'

export type OrderStatus = 'Draft' | 'Placed' | 'Canceled' | 'Expired'

export type CheckoutState = 'Address' | 'PickDeliveryMethod' | 'PickPaymentMethod' | 'Confirm' | 'Complete'

export type OrderPaymentState = 'Completed' | 'Failed' | 'Void' | 'BalanceDue' | 'CreditOwed' | 'Paid' | 'Pending' | 'Checkout' | 'Invalid'

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
  paymentState?: OrderPaymentState
  fulfillmentState?: OrderFulfillmentState
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
  shippingAdjustment?: {
    id: string
    label: string
    amount: number
    shippingMethodId?: string
  } | null
  total: number
  paymentTotal: number
  outstandingBalance: number
  paymentState?: OrderPaymentState
  fulfillmentState?: OrderFulfillmentState
  userId?: string
  itemCount: number
  approvedById?: string
  approvedAtUtc?: string
  completedAtUtc?: string
  canceledAtUtc?: string
  createdAtUtc: string
  modifiedAtUtc?: string
  paymentProcessingAtUtc?: string
  paymentCompletedAtUtc?: string
  paymentFailedAtUtc?: string
  shipmentShippedAtUtc?: string
  shipmentDeliveredAtUtc?: string
  lineItems: LineItem[]
  payments: PaymentCaptureSummary[]
  shipments: ShipmentSummary[]
  timeline: OrderTimelineEvent[]
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

export type OrderFulfillmentState = 'None' | 'Pending' | 'Partial' | 'Shipped' | 'Delivered' | 'Canceled'

export type ShipmentStatus = 'Pending' | 'Ready' | 'Shipped' | 'Delivered' | 'Backorder' | 'Canceled'

export interface Shipment {
  id: string
  orderId: string
  shippingMethodId: string
  trackingNumber: string | null
  status: ShipmentStatus
  shippedAtUtc: string | null
  deliveredAtUtc: string | null
  estimatedDeliveryAtUtc: string | null
  createdAtUtc: string
}

export interface PaymentCaptureSummary {
  id: string
  number: string
  amount: number
  currency: string
  state: string
  paymentStatus: string | null
  providerKey: string
  paymentMethodId: string | null
  createdAtUtc: string
  completedAtUtc: string | null
  failedAtUtc: string | null
}

export interface ShipmentSummary {
  id: string
  orderId: string
  shippingMethodId: string
  shippingMethodName: string | null
  trackingNumber: string | null
  status: ShipmentStatus
  shippedAtUtc: string | null
  deliveredAtUtc: string | null
  estimatedDeliveryAtUtc: string | null
  createdAtUtc: string
}

export interface OrderTimelineEvent {
  type: string
  label: string
  occurredAtUtc: string | null
}
