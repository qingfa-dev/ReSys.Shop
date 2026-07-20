import apiClient from '@/common/api/http/api.client'
import { LOCATIONS } from '@/common/api/constants'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { State } from '../types/state.response.type'
import type { CreateStateRequest, UpdateStateRequest } from '../types/state.request.type'
function path(sub?: string): string {
  return `${LOCATIONS}/states${sub ? `/${sub}` : ''}`
}

export const stateRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<State>> {
    return apiClient.get(path(), { params }).then(res => res.data as ServerPagedResult<State>)
  },
  async getById(id: string): Promise<ServerResult<State>> {
    return apiClient.get(path(id)).then(res => res.data as ServerResult<State>)
  },
  async create(data: CreateStateRequest): Promise<ServerResult<State>> {
    return apiClient.post(path(), data).then(res => res.data as ServerResult<State>)
  },
  async update(id: string, data: UpdateStateRequest): Promise<ServerResult<State>> {
    return apiClient.put(path(id), data).then(res => res.data as ServerResult<State>)
  },
  async delete(id: string): Promise<ServerResult<void>> {
    const res = await apiClient.delete(path(id))
    return res.data as ServerResult<void>
  },
  async getByIso(isoCode: string): Promise<ServerResult<State>> {
    return apiClient.get(path(`by-iso/${isoCode}`)).then(res => res.data as ServerResult<State>)
  },
}
