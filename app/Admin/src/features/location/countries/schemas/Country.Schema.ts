import { z } from 'zod'

export const CountrySchema = z.object({
  name: z.string().min(1, 'Country name is required').max(100, 'Country name must not exceed 100 characters'),
  isoCode: z.string().length(2, 'ISO code must be exactly 2 characters').toUpperCase(),
  callingCode: z.string().max(10, 'Calling code must not exceed 10 characters').default(''),
  isActive: z.boolean().default(true),
})

export type CountryParameters = z.infer<typeof CountrySchema>
