import { locationRepository } from '../repository/location.repository'
import { mapStateResponse } from '../mapper/location.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { State } from '../types/location.domain.types'
import type { StateCreateRequest, StateUpdateRequest } from '../types/location.request.types'

export const stateService = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<State>> {
    return locationRepository.states.list(params)
  },

  getById(id: string): Promise<ServerResult<State>> {
    return locationRepository.states.getById(id)
  },

  async create(data: StateCreateRequest): Promise<ServerResult<State>> {
    const result = await locationRepository.states.create(data)
    if (result.isSuccess) {
      return { ...result, value: mapStateResponse(result.value) }
    }
    return result
  },

  async update(id: string, data: StateUpdateRequest): Promise<ServerResult<State>> {
    const result = await locationRepository.states.update(id, data)
    if (result.isSuccess) {
      return { ...result, value: mapStateResponse(result.value) }
    }
    return result
  },

  delete(id: string): Promise<ServerResult<void>> {
    return locationRepository.states.delete(id)
  },
}
