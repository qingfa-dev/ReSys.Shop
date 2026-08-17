import type { AdjustmentSummary, ShippingAdjustmentSummary, ShippingCalculationSummary } from './cart'

export type OrderStatus = 'Draft' | 'Placed' | 'Canceled' | 'Expired'
export type CheckoutState = 'Address' | 'PickDeliveryMethod' | 'PickPaymentMethod' | 'Confirm' | 'Complete'
export type OrderPaymentState = 'Completed' | 'Failed' | 'Void' | 'BalanceDue' | 'CreditOwed' | 'Paid' | 'Pending' | 'Checkout' | 'Invalid'
export type OrderFulfillmentState = 'None' | 'Pending' | 'Partial' | 'Shipped' | 'Delivered' | 'Canceled'

export interface OrderListItem {
  id: string
  number: string
  status: OrderStatus
  total: number
  currency: string
  itemCount: number
  createdAtUtc: string
}

export interface OrderLineItem {
  id: string
  variantId: string | null
  productId: string | null
  productName: string | null
  productImageUrl: string | null
  quantity: number
  price: number
  total: number
  adjustmentTotal: number
  currency: string
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
  shippingAdjustment: ShippingAdjustmentSummary | null
  shippingCalculation: ShippingCalculationSummary | null
  adjustments: AdjustmentSummary[]
  paymentTotal: number
  outstandingBalance: number
  paymentState: OrderPaymentState | null
  fulfillmentState: OrderFulfillmentState | null
  userId: string | null
  approvedById: string | null
  approvedAtUtc: string | null
  completedAtUtc: string | null
  canceledAtUtc: string | null
  modifiedAtUtc: string | null
  specialInstructions: string | null
  lineItems: OrderLineItem[]
  paymentProcessingAtUtc: string | null
  paymentCompletedAtUtc: string | null
  paymentFailedAtUtc: string | null
  shipmentShippedAtUtc: string | null
  shipmentDeliveredAtUtc: string | null
  payments: PaymentCaptureSummary[]
  shipments: ShipmentSummary[]
  timeline: OrderTimelineEvent[]
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

export type ShipmentStatus = 'Pending' | 'Ready' | 'Shipped' | 'Delivered' | 'Backorder' | 'Canceled'

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

export interface OrderTrackingResponse {
  orderId: string
  orderCreatedAt: string
  orderApprovedAt: string | null
  orderCompletedAt: string | null
  orderCanceledAt: string | null
  shippedAt: string | null
  deliveredAt: string | null
  estimatedDeliveryAt: string | null
  paymentProcessingAt: string | null
  paymentCompletedAt: string | null
  paymentFailedAt: string | null
  deliveryExceptionAt: string | null
}
