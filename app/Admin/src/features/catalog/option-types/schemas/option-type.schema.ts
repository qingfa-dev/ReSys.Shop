import { z } from 'zod'

export const OptionTypeSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  presentation: z.string().min(1, 'Presentation is required').max(100),
  description: z.string().max(500).optional().nullable(),
  filterable: z.boolean().default(false),
  position: z.number().int().min(0).default(0),
})

export type OptionTypeFormData = z.infer<typeof OptionTypeSchema>
