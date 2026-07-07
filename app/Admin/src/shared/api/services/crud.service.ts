import apiClient from '../http/api.client'
import type { ApiResult } from '../types/api.types'
import type { ServerQueryingParameters } from '../types/query-params.types'

export function createCrudService<T, TCreate = Partial<T>, TUpdate = Partial<T>>(basePath: string) {
  return {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<T[]>> {
      return apiClient.get(basePath, { params })
    },

    async getById(id: string): Promise<ApiResult<T>> {
      return apiClient.get(`${basePath}/${id}`)
    },

    async create(data: TCreate): Promise<ApiResult<T>> {
      return apiClient.post(basePath, data)
    },

    async update(id: string, data: TUpdate): Promise<ApiResult<T>> {
      return apiClient.put(`${basePath}/${id}`, data)
    },

    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${basePath}/${id}`)
    },
  }
}
