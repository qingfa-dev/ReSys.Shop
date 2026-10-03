import { z } from 'zod'

export const OptionValueSchema = z.object({
  id: z.string(),
  name: z.string(),
  presentation: z.string().nullable(),
  position: z.number().int().min(0),
  optionTypeId: z.string(),
  optionTypeName: z.string().nullable(),
})

export const OptionTypeSchema = z.object({
  id: z.string(),
  name: z.string(),
  presentation: z.string().nullable(),
  position: z.number().int().min(0),
  filterable: z.boolean(),
})
