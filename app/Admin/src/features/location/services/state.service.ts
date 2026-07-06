import apiClient from '@/shared/api/api.client'
import type { ApiResult } from '@/shared/api/api.types'
import type { State, StateCreateRequest, StateUpdateRequest } from '../types/state.types'

export const stateService = {
  async getAll(params?: Record<string, unknown>): Promise<ApiResult<State[]>> {
    return apiClient.get('/api/admin/location/states', { params })
  },
  async getById(id: string): Promise<ApiResult<State>> {
    return apiClient.get(`/api/admin/location/states/${id}`)
  },
  async create(data: StateCreateRequest): Promise<ApiResult<State>> {
    return apiClient.post('/api/admin/location/states', data)
  },
  async update(id: string, data: StateUpdateRequest): Promise<ApiResult<State>> {
    return apiClient.put(`/api/admin/location/states/${id}`, data)
  },
  async delete(id: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/api/admin/location/states/${id}`)
  },
}
