import { countryRepository } from '../api/country.api'
import { mapCountryResponse } from '../mappers/country.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { Country } from '../types/Country.Response.Type'
import type { CreateCountryRequest, UpdateCountryRequest } from '../types/Country.Request.Type'

export const countryService = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<Country>> {
    return countryRepository.list(params)
  },

  getById(id: string): Promise<ServerResult<Country>> {
    return countryRepository.getById(id)
  },

  async create(data: CreateCountryRequest): Promise<ServerResult<Country>> {
    const result = await countryRepository.create(data)
    if (result.isSuccess) {
      return { ...result, value: mapCountryResponse(result.value) }
    }
    return result
  },

  async update(id: string, data: UpdateCountryRequest): Promise<ServerResult<Country>> {
    const result = await countryRepository.update(id, data)
    if (result.isSuccess) {
      return { ...result, value: mapCountryResponse(result.value) }
    }
    return result
  },

  delete(id: string): Promise<ServerResult<void>> {
    return countryRepository.delete(id)
  },
}
