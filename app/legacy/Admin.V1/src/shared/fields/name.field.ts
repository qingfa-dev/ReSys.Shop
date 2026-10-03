import { z } from 'zod'

export const nameFields = z.object({
  name: z.string().min(1),
  slug: z.string().optional(),
  description: z.string().optional(),
})

export type NameFields = z.infer<typeof nameFields>
