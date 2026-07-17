import { z } from 'zod'

export const OptionValueSchema = z.object({
  name: z
    .string()
    .min(1, 'Internal name is required')
    .max(100, 'Name must not exceed 100 characters'),
  presentation: z
    .string()
    .min(1, 'Display name is required')
    .max(100, 'Display name must not exceed 100 characters'),
  position: z
    .number()
    .int('Position must be a whole number')
    .min(0, 'Position must be non-negative')
    .default(0),
})

export type OptionValueParameters = z.infer<typeof OptionValueSchema>
