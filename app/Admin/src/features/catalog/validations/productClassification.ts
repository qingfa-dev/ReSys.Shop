import { z } from 'zod'

export const productClassificationTaxonId = z.string()
  .min(1, 'Taxon ID is required.')

export const productClassificationPosition = z.number()
  .int()
  .min(0, 'Position must be at least 0.')

export const productClassificationItemSchema = z.object({
  taxonId: productClassificationTaxonId,
  position: productClassificationPosition,
})

export const productClassificationSchema = z.object({
  productId: z.string().min(1, 'Product is required.'),
  items: z.array(productClassificationItemSchema).min(1, 'At least one taxon is required.'),
})

export type ProductClassificationForm = z.infer<typeof productClassificationSchema>
