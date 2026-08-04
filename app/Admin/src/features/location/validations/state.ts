import { z } from 'zod'

export const stateName = z.string()
  .min(1, 'State name is required.')
  .max(100, 'State name must not exceed 100 characters.')

export const stateAbbreviation = z.string()
  .min(1, 'Abbreviation is required.')
  .max(10, 'Abbreviation must not exceed 10 characters.')

export const stateCountryId = z.string()
  .min(1, 'Country is required.')

export const stateIsActive = z.boolean()

export const stateSchema = z.object({
  name: stateName,
  abbreviation: stateAbbreviation,
  countryId: stateCountryId,
  isActive: stateIsActive,
})

export type StateForm = z.infer<typeof stateSchema>
