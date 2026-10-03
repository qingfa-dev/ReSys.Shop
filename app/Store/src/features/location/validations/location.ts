import { z } from 'zod'

// Validate: Country shape matches Module.Location storefront DTO contract.
export const CountrySchema = z.object({
  id: z.string(),
  name: z.string(),
  isoCode: z.string(),
  callingCode: z.string().nullable(),
  statesRequired: z.boolean(),
  isActive: z.boolean(),
})

// Validate: State shape includes countryId FK for client-side cascade filtering.
export const StateSchema = z.object({
  id: z.string(),
  name: z.string(),
  abbreviation: z.string(),
  countryId: z.string(),
  isActive: z.boolean(),
  countryName: z.string().nullable(),
})
