import { z } from 'zod'
import { createCreateProductSchema } from './create-product.field'
export function createUpdateProductSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return createCreateProductSchema(t).partial()
}
export type UpdateProductParameters = z.infer<ReturnType<typeof createUpdateProductSchema>>
