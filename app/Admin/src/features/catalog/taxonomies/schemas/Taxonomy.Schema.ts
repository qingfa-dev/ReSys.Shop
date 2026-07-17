import { z } from 'zod'

export const TaxonomySchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  presentation: z.string().min(1, 'Presentation is required').max(100),
  position: z.number().int().min(0).default(0),
})

export type TaxonomyParameters = z.infer<typeof TaxonomySchema>
