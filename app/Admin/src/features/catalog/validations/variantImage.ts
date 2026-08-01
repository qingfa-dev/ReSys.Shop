import { z } from 'zod'

export const variantImageAlt = z.string()
  .max(500, 'Alt text cannot exceed 500 characters.')
  .optional()

export const variantImagePosition = z.number()
  .int()
  .min(0, 'Position must be greater than or equal to 0.')

export const variantImageType = z.enum(['Default', 'Thumbnail', 'Square', 'Gallery', 'Search'])
  .optional()

export const variantImageSchema = z.object({
  alt: variantImageAlt,
  position: variantImagePosition,
  type: variantImageType,
})

export type VariantImageForm = z.infer<typeof variantImageSchema>
