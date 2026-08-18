import { z } from 'zod'
import { PaymentRecordStateSchema } from './order'

// Validate: Checkout request schemas — partial updates for address, shipping, and payment steps.
export const UpdateCheckoutRequestSchema = z.object({
  shipAddressId: z.string().optional(),
  billAddressId: z.string().optional(),
  currency: z.string().optional(),
  email: z.string().optional(),
})

export const SelectShippingRateRequestSchema = z.object({
  shippingMethodId: z.string().min(1),
})

export const CreatePaymentIntentRequestSchema = z.object({
  orderId: z.string().min(1),
  paymentMethodId: z.string().min(1),
  returnUrl: z.string().url().optional(),
  cancelUrl: z.string().url().optional(),
})

export const PlaceOrderRequestSchema = z.object({
  paymentIntentId: z.string().optional(),
})

export const PlaceOrderResponseSchema = z.object({
  id: z.string(),
})

export const PaymentIntentResponseSchema = z.object({
  id: z.string(),
  clientSecret: z.string().nullable().optional(),
  responseCode: z.string().nullable().optional(),
  checkoutUrl: z.string().nullable().optional(),
  state: PaymentRecordStateSchema.nullable().optional(),
})
