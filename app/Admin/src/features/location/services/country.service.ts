import { locationRepository } from '../repository/location.repository'
import { mapCountryResponse } from '../mapper/location.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { Country } from '../types/location.domain.types'
import type { CountryCreateRequest, CountryUpdateRequest } from '../types/location.request.types'

export const countryService = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<Country>> {
    return locationRepository.countries.list(params)
  },

  getById(id: string): Promise<ServerResult<Country>> {
    return locationRepository.countries.getById(id)
  },

  async create(data: CountryCreateRequest): Promise<ServerResult<Country>> {
    const result = await locationRepository.countries.create(data)
    if (result.isSuccess) {
      return { ...result, value: mapCountryResponse(result.value) }
    }
    return result
  },

  async update(id: string, data: CountryUpdateRequest): Promise<ServerResult<Country>> {
    const result = await locationRepository.countries.update(id, data)
    if (result.isSuccess) {
      return { ...result, value: mapCountryResponse(result.value) }
    }
    return result
  },

  delete(id: string): Promise<ServerResult<void>> {
    return locationRepository.countries.delete(id)
  },
}
