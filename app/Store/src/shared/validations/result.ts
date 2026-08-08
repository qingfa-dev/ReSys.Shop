import { z } from 'zod'
import type { ApiError } from '@/shared/types/error'

/** Validates API error shape: code, message, type, optional field-level error. */
export const ApiErrorSchema = z.object({
  code: z.string(),
  message: z.string(),
  type: z.number(),
  field: z.string().optional(),
})

/** Validates single-value API Result envelope — wraps domain-specific value schemas. */
export function ResultSchema<T extends z.ZodType>(valueSchema: T) {
  return z.object({
    isSuccess: z.boolean(),
    statusCode: z.number(),
    message: z.string().nullable(),
    errors: z.array(ApiErrorSchema),
    value: valueSchema,
  })
}

// Validate: Paginated Result envelope — message is nullable per PagedResult<T> interface
export function PagedResultSchema<T extends z.ZodType>(itemSchema: T) {
  return z.object({
    isSuccess: z.boolean(),
    statusCode: z.number(),
    message: z.string().nullable().default(null),
    errors: z.array(ApiErrorSchema).default([]),
    items: z.array(itemSchema),
    page: z.number(),
    pageSize: z.number(),
    totalCount: z.number(),
    totalPages: z.number().optional().default(0),
  })
}
