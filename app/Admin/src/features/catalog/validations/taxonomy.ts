import { z } from 'zod'

export const taxonomyName = z.string()
  .min(1, 'Taxonomy name is required.')
  .max(100, 'Taxonomy name must not exceed 100 characters.')

export const taxonomyPresentation = z.string()
  .min(1, 'Presentation is required.')
  .max(100, 'Presentation must not exceed 100 characters.')

export const taxonomyPosition = z.number()
  .int()
  .min(-1, 'Position must be at least -1.')

export const taxonomySchema = z.object({
  name: taxonomyName,
  presentation: taxonomyPresentation,
  position: taxonomyPosition,
})

export type TaxonomyForm = z.infer<typeof taxonomySchema>
