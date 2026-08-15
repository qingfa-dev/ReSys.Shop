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
