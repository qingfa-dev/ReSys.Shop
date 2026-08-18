import { z } from 'zod'

// Validate: Order status and checkout state enum schemas for runtime type safety.
export const OrderStatusSchema = z.enum(['Draft', 'Placed', 'Canceled', 'Expired'])
export const CheckoutStateSchema = z.enum(['Address', 'PickDeliveryMethod', 'PickPaymentMethod', 'Confirm', 'Complete'])
export const OrderPaymentStateSchema = z.enum(['Completed', 'Failed', 'Void', 'BalanceDue', 'CreditOwed', 'Paid', 'Pending', 'Checkout', 'Invalid'])
export const OrderFulfillmentStateSchema = z.enum(['None', 'Pending', 'Partial', 'Shipped', 'Delivered', 'Canceled'])
export const PaymentRecordStateSchema = z.enum(['Checkout', 'Processing', 'Pending', 'Completed', 'Failed', 'Void', 'Disputed', 'Invalid'])

// Validate: Shipping adjustment summary shape shared by cart and order schemas.
export const ShippingAdjustmentSummarySchema = z.object({
  id: z.string(),
  label: z.string(),
  amount: z.number(),
  shippingMethodId: z.string().nullable(),
})

// Validate: Adjustment row shape shared by cart and order schemas.
export const AdjustmentSummarySchema = z.object({
  id: z.string(),
  label: z.string(),
  amount: z.number(),
  sourceType: z.string(),
  shippingMethodId: z.string().nullable(),
})

// Validate: Shipping calculation metadata shape shared by cart and order schemas.
export const ShippingCalculationSummarySchema = z.object({
  totalWeight: z.number().min(0),
  shippingRateId: z.string().nullable(),
  cost: z.number().min(0),
  isFreeShipping: z.boolean(),
})

export const OrderListItemSchema = z.object({
  id: z.string(),
  number: z.string(),
  status: OrderStatusSchema,
  total: z.number(),
  currency: z.string(),
  itemCount: z.number().int().min(0),
  createdAtUtc: z.string(),
})

export const OrderLineItemSchema = z.object({
  id: z.string(),
  variantId: z.string().nullable(),
  productId: z.string().nullable(),
  productName: z.string().nullable(),
  productImageUrl: z.string().nullable(),
  quantity: z.number(),
  price: z.number(),
  total: z.number(),
  adjustmentTotal: z.number(),
  currency: z.string(),
  createdAtUtc: z.string(),
})

export const PaymentCaptureSummarySchema = z.object({
  id: z.string(),
  number: z.string(),
  amount: z.number(),
  currency: z.string(),
  state: PaymentRecordStateSchema,
  paymentStatus: z.string().nullable(),
  providerKey: z.string(),
  paymentMethodId: z.string().nullable(),
  createdAtUtc: z.string(),
  completedAtUtc: z.string().nullable(),
  failedAtUtc: z.string().nullable(),
})

export const ShipmentStatusSchema = z.enum(['Pending', 'Ready', 'Shipped', 'Delivered', 'Backorder', 'Canceled'])

export const ShipmentSummarySchema = z.object({
  id: z.string(),
  orderId: z.string(),
  shippingMethodId: z.string(),
  shippingMethodName: z.string().nullable(),
  trackingNumber: z.string().nullable(),
  status: ShipmentStatusSchema,
  shippedAtUtc: z.string().nullable(),
  deliveredAtUtc: z.string().nullable(),
  estimatedDeliveryAtUtc: z.string().nullable(),
  createdAtUtc: z.string(),
})

export const OrderTimelineEventSchema = z.object({
  type: z.string(),
  label: z.string(),
  occurredAtUtc: z.string().nullable(),
})

export const OrderDetailSchema = z.object({
  id: z.string(),
  number: z.string(),
  status: OrderStatusSchema,
  total: z.number(),
  createdAtUtc: z.string(),
  checkoutState: CheckoutStateSchema,
  currency: z.string(),
  itemCount: z.number().int().min(0),
  email: z.string().nullable(),
  shipAddressId: z.string().nullable(),
  billAddressId: z.string().nullable(),
  shippingMethodId: z.string().nullable(),
  itemTotal: z.number(),
  adjustmentTotal: z.number(),
  shipmentTotal: z.number(),
  shippingAdjustment: ShippingAdjustmentSummarySchema.nullable(),
  shippingCalculation: ShippingCalculationSummarySchema.nullable(),
  adjustments: z.array(AdjustmentSummarySchema),
  paymentTotal: z.number(),
  outstandingBalance: z.number(),
  paymentState: OrderPaymentStateSchema.nullable(),
  fulfillmentState: OrderFulfillmentStateSchema.nullable(),
  userId: z.string().nullable(),
  approvedById: z.string().nullable(),
  approvedAtUtc: z.string().nullable(),
  completedAtUtc: z.string().nullable(),
  canceledAtUtc: z.string().nullable(),
  modifiedAtUtc: z.string().nullable(),
  specialInstructions: z.string().nullable(),
  paymentProcessingAtUtc: z.string().nullable(),
  paymentCompletedAtUtc: z.string().nullable(),
  paymentFailedAtUtc: z.string().nullable(),
  shipmentShippedAtUtc: z.string().nullable(),
  shipmentDeliveredAtUtc: z.string().nullable(),
  payments: z.array(PaymentCaptureSummarySchema),
  shipments: z.array(ShipmentSummarySchema),
  timeline: z.array(OrderTimelineEventSchema),
  lineItems: z.array(OrderLineItemSchema),
})

export const OrderTrackingResponseSchema = z.object({
  orderId: z.string(),
  orderCreatedAt: z.string(),
  orderApprovedAt: z.string().nullable(),
  orderCompletedAt: z.string().nullable(),
  orderCanceledAt: z.string().nullable(),
  shippedAt: z.string().nullable(),
  deliveredAt: z.string().nullable(),
  estimatedDeliveryAt: z.string().nullable(),
  paymentProcessingAt: z.string().nullable(),
  paymentCompletedAt: z.string().nullable(),
  paymentFailedAt: z.string().nullable(),
  deliveryExceptionAt: z.string().nullable(),
})
