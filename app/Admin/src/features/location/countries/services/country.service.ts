import { countryRepository } from '../api/country.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { Country } from '../types/country.response.type'
import type { CreateCountryRequest, UpdateCountryRequest } from '../types/country.request.type'

export const countryService = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<Country>> {
    return countryRepository.list(params)
  },

  getById(id: string): Promise<ServerResult<Country>> {
    return countryRepository.getById(id)
  },

  create(data: CreateCountryRequest): Promise<ServerResult<Country>> {
    return countryRepository.create(data)
  },

  update(id: string, data: UpdateCountryRequest): Promise<ServerResult<Country>> {
    return countryRepository.update(id, data)
  },

  delete(id: string): Promise<ServerResult<void>> {
    return countryRepository.delete(id)
  },
}
