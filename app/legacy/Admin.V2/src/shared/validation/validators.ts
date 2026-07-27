import { z } from 'zod'

export const baseField = z.object({
  id: z.string().uuid().optional(),
})

export const namedField = baseField.extend({
  name: z.string().min(1, 'Name is required').max(256),
})

export const activatedField = baseField.extend({
  isActive: z.boolean().default(true),
})

export const seoField = z.object({
  metaTitle: z.string().max(256).optional(),
  metaDescription: z.string().max(1024).optional(),
  metaKeywords: z.string().max(512).optional(),
})

export const sortableField = z.object({
  position: z.number().int().min(0).default(0),
})

export const moneyField = z.object({
  amount: z.number().min(0, 'Amount must be non-negative'),
  currency: z.string().length(3).default('USD'),
})

export type BaseField = z.infer<typeof baseField>
export type NamedField = z.infer<typeof namedField>
export type ActivatedField = z.infer<typeof activatedField>
export type SeoField = z.infer<typeof seoField>
export type SortableField = z.infer<typeof sortableField>
export type MoneyField = z.infer<typeof moneyField>
