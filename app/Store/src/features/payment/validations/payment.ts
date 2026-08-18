import { z } from 'zod'
import { PaymentRecordStateSchema } from '@/features/ordering/validations/order'

// Validate: PaymentMethod shape matches StorePaymentMethodListItemResponse from backend
export const PaymentMethodSchema = z.object({
  id: z.string(),
  name: z.string(),
  code: z.string().nullable(),
  description: z.string().nullable(),
  providerKey: z.string(),
  settings: z.record(z.string(), z.string()).nullable().optional(),
  preferences: z.record(z.string(), z.string()).nullable(),
  active: z.boolean(),
  autoCapture: z.boolean(),
  displayOn: z.enum(['Both', 'Frontend', 'Backend']),
  position: z.number().int(),
  presentation: z.string().nullable(),
  webhookEnabled: z.boolean(),
})

// Validate: PaymentIntent shape matches StorePaymentDetailResponse from backend
export const PaymentIntentSchema = z.object({
  id: z.string(),
  amount: z.number(),
  currency: z.string(),
  orderId: z.string(),
  paymentMethodId: z.string(),
  state: PaymentRecordStateSchema,
  paymentStatus: z.string().nullable(),
  clientSecret: z.string().nullable(),
  checkoutUrl: z.string().nullable().optional(),
  responseCode: z.string().nullable(),
  createdAtUtc: z.string(),
  modifiedAtUtc: z.string().nullable(),
})

// Validate: CreatePaymentIntentRequest requires orderId; backend derives amount/currency
export const CreatePaymentIntentRequestSchema = z.object({
  orderId: z.string(),
  paymentMethodId: z.string().optional(),
  returnUrl: z.string().url().optional(),
})

// Validate: CreateSetupIntentRequest requires paymentMethodId only
export const CreateSetupIntentRequestSchema = z.object({
  paymentMethodId: z.string(),
})

// Validate: ConfirmPaymentResponse extends PaymentIntent with confirmation message
export const ConfirmPaymentResponseSchema = PaymentIntentSchema.extend({
  message: z.string(),
})

// Validate: PaymentStatusResponse matches GET api/storefront/cart/payment/intent/{orderId}
export const PaymentStatusResponseSchema = z.object({
  id: z.string(),
  orderId: z.string(),
  amount: z.number(),
  currency: z.string(),
  state: PaymentRecordStateSchema,
  isCompleted: z.boolean(),
})
