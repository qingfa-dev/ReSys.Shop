import { z } from 'zod'

export const SearchByImageResponseSchema = z.object({
  variantId: z.string(),
  productId: z.string(),
  productName: z.string(),
  sku: z.string(),
  price: z.number(),
  imageUrl: z.string().nullable(),
  similarityScore: z.number().min(0).max(1),
})

export const VisualSearchModelSchema = z.object({
  id: z.string(),
  name: z.string(),
  description: z.string().nullable(),
  dimension: z.number(),
  isOnnx: z.boolean(),
})
