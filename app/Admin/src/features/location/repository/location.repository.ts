import apiClient from '@/shared/api/http/api.client'
import { LOCATIONS } from '@/shared/api/constants'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { Country } from '../types/Country.Response.Type'
import type { State } from '../types/State.Response.Type'
import type { CreateCountryRequest, UpdateCountryRequest } from '../types/Country.Request.Type'
import type { CreateStateRequest, UpdateStateRequest } from '../types/State.Request.Type'

function countriesPath(sub?: string): string {
  return `${LOCATIONS}/countries${sub ? `/${sub}` : ''}`
}

function statesPath(sub?: string): string {
  return `${LOCATIONS}/states${sub ? `/${sub}` : ''}`
}

export const locationRepository = {
  countries: {
    async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<Country>> {
      const res = await apiClient.get(countriesPath(), { params })
      return res.data as ServerPagedResult<Country>
    },
    async getById(id: string): Promise<ServerResult<Country>> {
      const res = await apiClient.get(countriesPath(id))
      return res.data as ServerResult<Country>
    },
    async create(data: CreateCountryRequest): Promise<ServerResult<Country>> {
      const res = await apiClient.post(countriesPath(), data)
      return res.data as ServerResult<Country>
    },
    async update(id: string, data: UpdateCountryRequest): Promise<ServerResult<Country>> {
      const res = await apiClient.put(countriesPath(id), data)
      return res.data as ServerResult<Country>
    },
    async delete(id: string): Promise<ServerResult<void>> {
      const res = await apiClient.delete(countriesPath(id))
      return res.data as ServerResult<void>
    },
  },

  states: {
    async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<State>> {
      const res = await apiClient.get(statesPath(), { params })
      return res.data as ServerPagedResult<State>
    },
    async getById(id: string): Promise<ServerResult<State>> {
      const res = await apiClient.get(statesPath(id))
      return res.data as ServerResult<State>
    },
    async create(data: CreateStateRequest): Promise<ServerResult<State>> {
      const res = await apiClient.post(statesPath(), data)
      return res.data as ServerResult<State>
    },
    async update(id: string, data: UpdateStateRequest): Promise<ServerResult<State>> {
      const res = await apiClient.put(statesPath(id), data)
      return res.data as ServerResult<State>
    },
    async delete(id: string): Promise<ServerResult<void>> {
      const res = await apiClient.delete(statesPath(id))
      return res.data as ServerResult<void>
    },
  },
}
