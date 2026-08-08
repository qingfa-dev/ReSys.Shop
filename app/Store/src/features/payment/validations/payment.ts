import { z } from 'zod'

export const PaymentMethodSchema = z.object({
  id: z.string(),
  name: z.string(),
  code: z.string().nullable(),
  description: z.string().nullable(),
  providerKey: z.string(),
  preferences: z.record(z.string(), z.string()).nullable(),
  active: z.boolean(),
  autoCapture: z.boolean(),
  displayOn: z.enum(['Both', 'Frontend', 'Backend']),
  position: z.number().int(),
  presentation: z.string().nullable(),
  webhookEnabled: z.boolean(),
})

export const PaymentIntentSchema = z.object({
  id: z.string(),
  amount: z.number(),
  currency: z.string(),
  orderId: z.string(),
  paymentMethodId: z.string(),
  state: z.string(),
  paymentStatus: z.string().nullable(),
  clientSecret: z.string().nullable(),
  responseCode: z.string().nullable(),
  createdAtUtc: z.string(),
  modifiedAtUtc: z.string().nullable(),
})

export const CreatePaymentIntentRequestSchema = z.object({
  orderId: z.string(),
  paymentMethodId: z.string().optional(),
  returnUrl: z.string().url().optional(),
})

export const CreateSetupIntentRequestSchema = z.object({
  paymentMethodId: z.string(),
})

export const ConfirmPaymentResponseSchema = PaymentIntentSchema.extend({
  message: z.string(),
})
