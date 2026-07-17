import { z } from 'zod'

export const PropertyKindEnum = z.enum(['String', 'Integer', 'Float', 'Boolean', 'Date', 'Html'])

export const PropertyTypeSchema = z.object({
  name: z
    .string()
    .min(1, 'Name is required')
    .max(100, 'Name must not exceed 100 characters'),
  presentation: z
    .string()
    .min(1, 'Presentation is required')
    .max(100, 'Presentation must not exceed 100 characters'),
  kind: PropertyKindEnum.default('String'),
  position: z
    .number()
    .int('Position must be a whole number')
    .min(0, 'Position must be non-negative')
    .default(0),
  filterable: z.boolean().default(false),
})

export type PropertyKind = z.infer<typeof PropertyKindEnum>

export type PropertyTypeParameters = z.infer<typeof PropertyTypeSchema>
