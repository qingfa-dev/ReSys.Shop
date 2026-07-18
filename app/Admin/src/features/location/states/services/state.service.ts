import { stateRepository } from '../api/state.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { State } from '../types/state.response.type'
import type { CreateStateRequest, UpdateStateRequest } from '../types/state.request.type'

export const stateService = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<State>> {
    return stateRepository.list(params)
  },

  getById(id: string): Promise<ServerResult<State>> {
    return stateRepository.getById(id)
  },

  create(data: CreateStateRequest): Promise<ServerResult<State>> {
    return stateRepository.create(data)
  },

  update(id: string, data: UpdateStateRequest): Promise<ServerResult<State>> {
    return stateRepository.update(id, data)
  },

  delete(id: string): Promise<ServerResult<void>> {
    return stateRepository.delete(id)
  },
}
