import { z } from 'zod'

export const PromotionFields = {
  Required: {
    id: z.string(),
    code: z.string(),
    type: z.enum(['percentage', 'fixed', 'bogo', 'shipping']),
    value: z.number(),
    usedCount: z.number(),
    startsAt: z.string(),
    expiresAt: z.string(),
    isActive: z.boolean(),
  },
  Optional: {
    minOrderAmount: z.number().optional(),
    maxUses: z.number().optional(),
    description: z.string().optional(),
    terms: z.string().optional(),
  },
} as const

export const PromotionSchema = z.object({
  ...PromotionFields.Required,
  ...PromotionFields.Optional,
})

export type PromotionSchemaType = z.infer<typeof PromotionSchema>

export const CouponFields = {
  Required: {
    code: z.string(),
    description: z.string(),
    discount: z.string(),
  },
  Optional: {
    expiresAt: z.string().optional(),
    minOrderAmount: z.number().optional(),
  },
} as const

export const CouponSchema = z.object({
  ...CouponFields.Required,
  ...CouponFields.Optional,
})

export type CouponSchemaType = z.infer<typeof CouponSchema>

export const ValidateCouponSchema = z.object({
  code: z.string(),
  orderTotal: z.number().optional(),
})

export type ValidateCouponSchemaType = z.infer<typeof ValidateCouponSchema>