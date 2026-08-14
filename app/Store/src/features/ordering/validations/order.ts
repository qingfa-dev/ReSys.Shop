import { z } from 'zod'

// Validate: Order status and checkout state enum schemas for runtime type safety.
export const OrderStatusSchema = z.enum(['Draft', 'Placed', 'Canceled', 'Expired'])
export const CheckoutStateSchema = z.enum(['Address', 'Delivery', 'Payment', 'Confirm', 'Complete'])

export const OrderListItemSchema = z.object({
  id: z.string(),
  number: z.string(),
  status: OrderStatusSchema,
  total: z.number(),
  createdAtUtc: z.string(),
})

export const OrderLineItemSchema = z.object({
  id: z.string(),
  variantId: z.string().nullable(),
  quantity: z.number(),
  price: z.number(),
  total: z.number(),
  currency: z.string(),
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
  email: z.string().nullable(),
  shipAddressId: z.string().nullable(),
  billAddressId: z.string().nullable(),
  shippingMethodId: z.string().nullable(),
  itemTotal: z.number(),
  adjustmentTotal: z.number(),
  shipmentTotal: z.number(),
  paymentTotal: z.number(),
  outstandingBalance: z.number(),
  paymentState: z.string().nullable(),
  shipmentState: z.string().nullable(),
  userId: z.string().nullable(),
  approvedById: z.string().nullable(),
  approvedAtUtc: z.string().nullable(),
  completedAtUtc: z.string().nullable(),
  canceledAtUtc: z.string().nullable(),
  modifiedAtUtc: z.string().nullable(),
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
})
