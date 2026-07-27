import { z } from 'zod'

export function statusSchema() {
  return z.number().int().min(0).optional()
}

import { createProductSchema } from './create-product.field'

export function updateProductSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return createProductSchema(t).extend({
    status: statusSchema(),
  })
}

export type UpdateProductParameters = z.infer<ReturnType<typeof updateProductSchema>>
