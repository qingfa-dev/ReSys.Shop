import { z } from 'zod'

import { ProductListItemSchema } from './product'

export const SearchByImageResponseSchema = ProductListItemSchema.extend({
  similarityScore: z.number().min(0).max(1),
})

export const VisualSearchModelSchema = z.object({
  id: z.string(),
  name: z.string(),
  description: z.string().nullable(),
  dimension: z.number(),
  isOnnx: z.boolean(),
})
