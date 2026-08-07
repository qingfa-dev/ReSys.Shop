import { z } from 'zod'

export const ApiErrorSchema = z.object({
  code: z.string(),
  message: z.string(),
  type: z.number(),
  metadata: z.record(z.unknown()).nullable().optional(),
})

export function ResultSchema<T extends z.ZodTypeAny>(valueSchema: T) {
  return z.object({
    isSuccess: z.boolean(),
    statusCode: z.number(),
    message: z.string().nullable(),
    errors: z.array(ApiErrorSchema),
    value: valueSchema,
  })
}

export function PagedResultSchema<T extends z.ZodTypeAny>(itemSchema: T) {
  return z.object({
    isSuccess: z.boolean(),
    statusCode: z.number(),
    message: z.string().nullable(),
    errors: z.array(ApiErrorSchema),
    items: z.array(itemSchema),
    page: z.number(),
    pageSize: z.number(),
    totalCount: z.number(),
    totalPages: z.number(),
  })
}
