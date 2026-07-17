import apiClient from '@/shared/api/http/api.client'
import { LOCATIONS } from '@/shared/api/constants'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { State } from '../../types/State.Response.Type'
import type { CreateStateRequest, UpdateStateRequest } from '../../types/State.Request.Type'

function path(sub?: string): string {
  return `${LOCATIONS}/states${sub ? `/${sub}` : ''}`
}

export const stateRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<State>> {
    const res = await apiClient.get(path(), { params })
    return res.data as ServerPagedResult<State>
  },
  async getById(id: string): Promise<ServerResult<State>> {
    const res = await apiClient.get(path(id))
    return res.data as ServerResult<State>
  },
  async create(data: CreateStateRequest): Promise<ServerResult<State>> {
    const res = await apiClient.post(path(), data)
    return res.data as ServerResult<State>
  },
  async update(id: string, data: UpdateStateRequest): Promise<ServerResult<State>> {
    const res = await apiClient.put(path(id), data)
    return res.data as ServerResult<State>
  },
  async delete(id: string): Promise<ServerResult<void>> {
    const res = await apiClient.delete(path(id))
    return res.data as ServerResult<void>
  },
}
