import { z } from 'zod'

export const seoFields = z.object({
  metaTitle: z.string().optional(),
  metaDescription: z.string().optional(),
  metaKeywords: z.string().optional(),
})

export type SeoFields = z.infer<typeof seoFields>
