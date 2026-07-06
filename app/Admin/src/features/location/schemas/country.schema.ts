import { z } from 'zod'
export const countryCreateSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  isoCode2: z.string().length(2, 'Must be exactly 2 characters'),
  isoCode3: z.string().length(3, 'Must be exactly 3 characters'),
  numericCode: z.string().default(''),
  phoneCode: z.string().default(''),
  isActive: z.boolean().default(true),
})
