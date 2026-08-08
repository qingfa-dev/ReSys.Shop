export type OrderStatus = 'Draft' | 'Placed' | 'Canceled' | 'Expired'
export type CheckoutState = 'Address' | 'Delivery' | 'Payment' | 'Confirm' | 'Complete'

export interface OrderListItem {
  id: string
  number: string
  status: OrderStatus
  total: number
  createdAtUtc: string
}

export interface OrderDetail extends OrderListItem {
  checkoutState: CheckoutState
  currency: string
  email: string | null
  shipAddressId: string | null
  billAddressId: string | null
  shippingMethodId: string | null
  itemTotal: number
  adjustmentTotal: number
  shipmentTotal: number
  paymentTotal: number
  outstandingBalance: number
  paymentState: string | null
  shipmentState: string | null
  userId: string | null
  storeId: string | null
  approvedById: string | null
  approvedAtUtc: string | null
  completedAtUtc: string | null
  canceledAtUtc: string | null
  modifiedAtUtc: string | null
}

export interface OrderTrackingResponse {
  orderId: string
  orderCreatedAt: string
  orderApprovedAt: string | null
  orderCompletedAt: string | null
  orderCanceledAt: string | null
  shippedAt: string | null
  deliveredAt: string | null
  estimatedDeliveryAt: string | null
}
