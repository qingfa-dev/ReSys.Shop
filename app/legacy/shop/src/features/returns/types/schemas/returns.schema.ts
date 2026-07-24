import { z } from 'zod'

export const ReturnRequestFields = {
  Required: {
    id: z.string(),
    orderId: z.string(),
    status: z.enum(['pending', 'approved', 'rejected', 'received', 'refunded']),
    items: z.array(z.object({
      orderItemId: z.string(),
      quantity: z.number(),
      reason: z.string(),
    })),
    refundAmount: z.number(),
    refundMethod: z.enum(['original', 'store_credit']),
    createdAt: z.string(),
    updatedAt: z.string(),
  },
  Optional: {
    trackingNumber: z.string().optional(),
    refundReason: z.string().optional(),
  },
} as const

export const ReturnRequestSchema = z.object({
  ...ReturnRequestFields.Required,
  ...ReturnRequestFields.Optional,
})

export type ReturnRequestSchemaType = z.infer<typeof ReturnRequestSchema>

export const CreateReturnRequestFields = {
  Required: {
    orderId: z.string(),
    items: z.array(z.object({
      orderItemId: z.string(),
      quantity: z.number(),
      reason: z.string(),
    })),
  },
  Optional: {
    refundMethod: z.enum(['original', 'store_credit']).optional(),
  },
} as const

export const CreateReturnRequestSchema = z.object({
  ...CreateReturnRequestFields.Required,
  ...CreateReturnRequestFields.Optional,
})

export type CreateReturnRequestSchemaType = z.infer<typeof CreateReturnRequestSchema>