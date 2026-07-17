import { z } from 'zod'
export const countryCreateSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  isoCode: z.string().length(2, 'Must be exactly 2 characters'),
  callingCode: z.string().default(''),
  isActive: z.boolean().default(true),
})
