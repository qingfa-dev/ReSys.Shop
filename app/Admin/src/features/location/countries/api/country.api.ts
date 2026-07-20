import apiClient from '@/common/api/http/api.client'
import { LOCATIONS } from '@/common/api/constants'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { Country } from '../types/country.response'
import type { CreateCountryRequest, UpdateCountryRequest } from '../types/country.request'
function path(sub?: string): string {
  return `${LOCATIONS}/countries${sub ? `/${sub}` : ''}`
}

export const countryRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<Country>> {
    return apiClient.get(path(), { params }).then(res => res.data as ServerPagedResult<Country>)
  },
  async getById(id: string): Promise<ServerResult<Country>> {
    return apiClient.get(path(id)).then(res => res.data as ServerResult<Country>)
  },
  async create(data: CreateCountryRequest): Promise<ServerResult<Country>> {
    return apiClient.post(path(), data).then(res => res.data as ServerResult<Country>)
  },
  async update(id: string, data: UpdateCountryRequest): Promise<ServerResult<Country>> {
    return apiClient.put(path(id), data).then(res => res.data as ServerResult<Country>)
  },
  async delete(id: string): Promise<ServerResult<void>> {
    const res = await apiClient.delete(path(id))
    return res.data as ServerResult<void>
  },
  async getByIso(isoCode: string): Promise<ServerResult<Country>> {
    return apiClient.get(path(`by-iso/${isoCode}`)).then(res => res.data as ServerResult<Country>)
  },
}
