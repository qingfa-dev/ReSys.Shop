import { z } from 'zod'

// Validate: Order status and checkout state enum schemas for runtime type safety.
export const OrderStatusSchema = z.enum(['Draft', 'Placed', 'Canceled', 'Expired'])
export const CheckoutStateSchema = z.enum(['Address', 'PickDeliveryMethod', 'PickPaymentMethod', 'Confirm', 'Complete'])
export const OrderPaymentStateSchema = z.enum(['Completed', 'Failed', 'Void', 'BalanceDue', 'CreditOwed', 'Paid', 'Pending', 'Checkout', 'Invalid'])
export const OrderFulfillmentStateSchema = z.enum(['None', 'Pending', 'Partial', 'Shipped', 'Delivered', 'Canceled'])

// Validate: Shipping adjustment summary shape shared by cart and order schemas.
export const ShippingAdjustmentSummarySchema = z.object({
  id: z.string(),
  label: z.string(),
  amount: z.number(),
  shippingMethodId: z.string().nullable(),
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
  quantity: z.number(),
  price: z.number(),
  total: z.number(),
  currency: z.string(),
  adjustmentTotal: z.number(),
  createdAtUtc: z.string(),
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
  shippingAdjustment: ShippingAdjustmentSummarySchema.nullable(),
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
