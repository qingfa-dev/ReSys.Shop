import apiClient from '@/shared/api/http/api.client'
import { LOCATIONS } from '@/shared/api/constants'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { Country } from '../../types/Country.Response.Type'
import type { CreateCountryRequest, UpdateCountryRequest } from '../../types/Country.Request.Type'

function path(sub?: string): string {
  return `${LOCATIONS}/countries${sub ? `/${sub}` : ''}`
}

export const countryRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<Country>> {
    const res = await apiClient.get(path(), { params })
    return res.data as ServerPagedResult<Country>
  },
  async getById(id: string): Promise<ServerResult<Country>> {
    const res = await apiClient.get(path(id))
    return res.data as ServerResult<Country>
  },
  async create(data: CreateCountryRequest): Promise<ServerResult<Country>> {
    const res = await apiClient.post(path(), data)
    return res.data as ServerResult<Country>
  },
  async update(id: string, data: UpdateCountryRequest): Promise<ServerResult<Country>> {
    const res = await apiClient.put(path(id), data)
    return res.data as ServerResult<Country>
  },
  async delete(id: string): Promise<ServerResult<void>> {
    const res = await apiClient.delete(path(id))
    return res.data as ServerResult<void>
  },
  async getByIso(isoCode: string): Promise<ServerResult<Country>> {
    const res = await apiClient.get(path(`by-iso/${isoCode}`))
    return res.data as ServerResult<Country>
  },
}
