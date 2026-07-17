import { z } from 'zod'

export const OptionTypeSchema = z.object({
  name: z
    .string()
    .min(1, 'Name is required')
    .max(100, 'Name must not exceed 100 characters'),
  presentation: z
    .string()
    .min(1, 'Presentation is required')
    .max(100, 'Presentation must not exceed 100 characters'),
  description: z
    .string()
    .max(500, 'Description must not exceed 500 characters')
    .optional()
    .nullable(),
  filterable: z.boolean().default(false),
  position: z
    .number()
    .int('Position must be a whole number')
    .min(0, 'Position must be non-negative')
    .default(0),
})

export type OptionTypeParameters = z.infer<typeof OptionTypeSchema>
