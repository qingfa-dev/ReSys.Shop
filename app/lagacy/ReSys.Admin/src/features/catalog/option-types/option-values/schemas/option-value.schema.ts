import { z } from 'zod'

export const OptionValueSchema = z.object({
  name: z.string().min(1, 'Internal name is required').max(100),
  presentation: z.string().min(1, 'Display name is required').max(100),
  position: z.number().int().min(0).default(0),
})

export type OptionValueFormData = z.infer<typeof OptionValueSchema>
