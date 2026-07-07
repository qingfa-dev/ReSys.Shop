import { createModuleApi } from '@/shared/api'
import type { Country, CountryCreateRequest, CountryUpdateRequest } from '../types/country.types'
import type { State, StateCreateRequest, StateUpdateRequest } from '../types/state.types'
import { LOCATION } from '@/shared/api/constants'

export const locationApi = {
  countries: createModuleApi<Country, CountryCreateRequest, CountryUpdateRequest>({ basePath: `${LOCATION}/countries` }),
  states: createModuleApi<State, StateCreateRequest, StateUpdateRequest>({ basePath: `${LOCATION}/states` }),
}
