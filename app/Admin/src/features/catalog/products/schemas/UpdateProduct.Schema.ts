import { z } from 'zod'
import { CreateProductSchema } from './CreateProduct.Schema'
export const UpdateProductSchema = CreateProductSchema.partial()
export type UpdateProductParameters = z.infer<typeof UpdateProductSchema>
