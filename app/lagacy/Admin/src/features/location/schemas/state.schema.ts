import { z } from 'zod'
export const stateCreateSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  abbreviation: z.string().min(1, 'Abbreviation is required'),
  countryId: z.string().min(1, 'Country is required'),
  isActive: z.boolean().default(true),
})
