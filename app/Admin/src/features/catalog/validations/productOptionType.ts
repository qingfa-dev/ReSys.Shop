import { z } from 'zod'

export const productOptionTypeOptionTypeId = z.string()
  .min(1, 'Option type ID is required.')

export const productOptionTypePosition = z.number()
  .int()
  .min(0, 'Position must be at least 0.')

export const productOptionTypeItemSchema = z.object({
  optionTypeId: productOptionTypeOptionTypeId,
  position: productOptionTypePosition,
})

export const productOptionTypeSchema = z.object({
  productId: z.string().min(1, 'Product is required.'),
  items: z.array(productOptionTypeItemSchema).min(1, 'At least one option type is required.'),
})

export type ProductOptionTypeForm = z.infer<typeof productOptionTypeSchema>
