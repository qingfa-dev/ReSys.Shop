// Types mirror the storefront order DTOs exactly (camelCase JSON):
// - List item: StorefrontOrderListItemResponse (id, number, status, total, createdAtUtc)
// - Detail:   OrderDetailResponse (full order header; no line items / timeline arrays on the wire)
// Enums serialize as strings via JsonStringEnumConverter.

export type OrderStatus = 'Draft' | 'Placed' | 'Canceled' | 'Expired'

export type CheckoutState = 'Address' | 'Delivery' | 'Payment' | 'Confirm' | 'Complete'

export interface OrderListItem {
  id: string
  number: string
  status: OrderStatus
  total: number
  createdAtUtc: string
}

export interface OrderDetail {
  id: string
  number: string
  status: OrderStatus
  checkoutState: CheckoutState
  currency: string
  email: string | null
  specialInstructions: string | null
  billAddressId: string | null
  shipAddressId: string | null
  shippingMethodId: string | null
  itemTotal: number
  adjustmentTotal: number
  shipmentTotal: number
  total: number
  paymentTotal: number
  outstandingBalance: number
  paymentState: string | null
  shipmentState: string | null
  userId: string | null
  storeId: string | null
  itemCount: number
  approvedById: string | null
  approvedAtUtc: string | null
  completedAtUtc: string | null
  canceledAtUtc: string | null
  createdAtUtc: string
  modifiedAtUtc: string | null
}

// Querying DSL constraints for GET api/storefront/orders (mirror OrderConstant.Query).
export const ORDER_FILTER_FIELDS = ['status', 'checkoutState', 'currency', 'userId', 'storeId', 'isDeleted']
export const ORDER_SORT_FIELDS = ['number', 'total', 'completedAtUtc', 'createdAtUtc', 'status']
export const ORDER_SEARCH_FIELDS = ['number', 'email']

// The storefront list/detail endpoints only expose placed orders; a placed order is the
// only status the customer can cancel (Draft/Expired are not listable, Canceled is terminal).
export function isOrderCancellable(status: OrderStatus): boolean {
  return status === 'Placed'
}

// Derive a state-transition timeline from the timestamps the detail DTO actually carries.
export interface OrderTimelineEntry {
  label: string
  date: string
  status: OrderStatus
}

export function buildOrderTimeline(order: OrderDetail): OrderTimelineEntry[] {
  const entries: OrderTimelineEntry[] = [{ label: 'Created', date: order.createdAtUtc, status: 'Placed' }]
  if (order.approvedAtUtc) entries.push({ label: 'Approved', date: order.approvedAtUtc, status: 'Placed' })
  if (order.completedAtUtc) entries.push({ label: 'Completed', date: order.completedAtUtc, status: 'Placed' })
  if (order.canceledAtUtc) entries.push({ label: 'Canceled', date: order.canceledAtUtc, status: 'Canceled' })
  if (order.modifiedAtUtc) entries.push({ label: 'Modified', date: order.modifiedAtUtc, status: 'Placed' })
  return entries
}
