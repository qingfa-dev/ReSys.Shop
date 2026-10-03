import { z } from 'zod'

export const countryName = z.string()
  .min(1, 'Country name is required.')
  .max(100, 'Country name must not exceed 100 characters.')

export const countryIsoCode = z.string()
  .min(1, 'ISO code is required.')
  .max(3, 'ISO code must not exceed 3 characters.')
  .regex(/^[A-Z]{2,3}$/, 'ISO code must be 2-3 uppercase letters.')

export const countryCallingCode = z.string()
  .max(10, 'Calling code must not exceed 10 characters.')

export const countryStatesRequired = z.boolean()
export const countryIsActive = z.boolean()

export const countrySchema = z.object({
  name: countryName,
  isoCode: countryIsoCode,
  callingCode: countryCallingCode.optional(),
  statesRequired: countryStatesRequired,
  isActive: countryIsActive,
})

export type CountryForm = z.infer<typeof countrySchema>
