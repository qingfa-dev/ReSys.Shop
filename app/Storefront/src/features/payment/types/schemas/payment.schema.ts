import { z } from 'zod'

export const PaymentIntentFields = {
  Required: {
    id: z.string(),
    amount: z.number(),
    currency: z.string(),
    status: z.enum(['pending', 'processing', 'succeeded', 'failed']),
  },
  Optional: {
    clientSecret: z.string().optional(),
    metadata: z.record(z.string()).optional(),
  },
} as const

export const PaymentIntentSchema = z.object({
  id: z.string(),
  amount: z.number(),
  currency: z.string(),
  status: z.enum(['pending', 'processing', 'succeeded', 'failed']),
  clientSecret: z.string().optional(),
  metadata: z.record(z.string()).optional(),
  responseCode: z.string().nullable().optional(),
})

export type PaymentIntentSchemaType = z.infer<typeof PaymentIntentSchema>

export const TransactionSchema = z.object({
  id: z.string(),
  orderId: z.string(),
  amount: z.number(),
  currency: z.string(),
  status: z.enum(['pending', 'completed', 'failed', 'refunded']),
  paymentMethod: z.string(),
  createdAt: z.string(),
})

export type TransactionSchemaType = z.infer<typeof TransactionSchema>

export const PaymentMethodSchema = z.object({
  id: z.string(),
  type: z.enum(['card', 'bank', 'wallet']),
  last4: z.string().optional(),
  brand: z.string().optional(),
  expiryMonth: z.number().optional(),
  expiryYear: z.number().optional(),
})

export type PaymentMethodSchemaType = z.infer<typeof PaymentMethodSchema>