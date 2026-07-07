import apiClient from '../http/api.client'
import { createCrudService } from './crud.service'
import type { ApiResult } from '../types/api.types'

export interface ModuleApiConfig {
  basePath: string
}

export function createModuleApi<T, TCreate = Partial<T>, TUpdate = Partial<T>>(config: ModuleApiConfig) {
  const crud = createCrudService<T, TCreate, TUpdate>(config.basePath)

  return {
    ...crud,

    getSubResource<T>(path: string, params?: Record<string, unknown>): Promise<ApiResult<T>> {
      return apiClient.get(`${config.basePath}/${path}`, { params })
    },

    postSubResource<T>(path: string, data?: unknown): Promise<ApiResult<T>> {
      return apiClient.post(`${config.basePath}/${path}`, data)
    },

    putSubResource<T>(path: string, data?: unknown): Promise<ApiResult<T>> {
      return apiClient.put(`${config.basePath}/${path}`, data)
    },

    deleteSubResource<T>(path: string, params?: Record<string, unknown>): Promise<ApiResult<T>> {
      return apiClient.delete(`${config.basePath}/${path}`, { params })
    },

    postAction<T>(path: string, data?: unknown): Promise<ApiResult<T>> {
      return apiClient.post(`${config.basePath}/${path}`, data)
    },
  }
}
