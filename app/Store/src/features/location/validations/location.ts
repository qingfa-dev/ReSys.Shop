import { z } from 'zod'

export const CountrySchema = z.object({
  id: z.string(),
  name: z.string(),
  isoCode: z.string(),
  callingCode: z.string().nullable(),
  statesRequired: z.boolean(),
  isActive: z.boolean(),
})

export const StateSchema = z.object({
  id: z.string(),
  name: z.string(),
  abbreviation: z.string(),
  countryId: z.string(),
  isActive: z.boolean(),
  countryName: z.string().nullable(),
})
