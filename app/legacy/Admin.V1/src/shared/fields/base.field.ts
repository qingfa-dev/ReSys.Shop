import { z } from 'zod'

export const baseFields = z.object({
  id: z.string(),
  createdAtUtc: z.string().optional(),
  updatedAtUtc: z.string().optional(),
})

export type BaseFields = z.infer<typeof baseFields>
