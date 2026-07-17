import type { z } from 'zod'
import { countryCreateSchema } from '../schemas/country.schema'
import { stateCreateSchema } from '../schemas/state.schema'

export type CountryFormData = z.infer<typeof countryCreateSchema>
export type StateFormData = z.infer<typeof stateCreateSchema>
