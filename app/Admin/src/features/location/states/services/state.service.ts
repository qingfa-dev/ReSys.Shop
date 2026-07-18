import { stateRepository } from '../api/state.api'
import { mapStateResponse } from '../mappers/state.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { State } from '../types/State.Response.Type'
import type { CreateStateRequest, UpdateStateRequest } from '../types/State.Request.Type'

export const stateService = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<State>> {
    return stateRepository.list(params)
  },

  getById(id: string): Promise<ServerResult<State>> {
    return stateRepository.getById(id)
  },

  async create(data: CreateStateRequest): Promise<ServerResult<State>> {
    const result = await stateRepository.create(data)
    if (result.isSuccess) {
      return { ...result, value: mapStateResponse(result.value) }
    }
    return result
  },

  async update(id: string, data: UpdateStateRequest): Promise<ServerResult<State>> {
    const result = await stateRepository.update(id, data)
    if (result.isSuccess) {
      return { ...result, value: mapStateResponse(result.value) }
    }
    return result
  },

  delete(id: string): Promise<ServerResult<void>> {
    return stateRepository.delete(id)
  },
}
