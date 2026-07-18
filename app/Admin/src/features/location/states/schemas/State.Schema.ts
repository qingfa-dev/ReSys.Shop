import { z } from 'zod'

export const StateSchema = z.object({
  name: z.string().min(1, 'State name is required').max(100, 'State name must not exceed 100 characters'),
  abbreviation: z.string().min(1, 'Abbreviation is required').max(10, 'Abbreviation must not exceed 10 characters'),
  countryId: z.string().uuid('Invalid country').min(1, 'Country is required'),
  isActive: z.boolean().default(true),
})

export type StateParameters = z.infer<typeof StateSchema>
