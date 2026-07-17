import apiClient from '../http/api.client'
import type { ServerResult } from '../types/result.types'

export interface ModuleApiConfig {
  basePath: string
}

export function createModuleApi<T, TCreate = Partial<T>, TUpdate = Partial<T>>(config: ModuleApiConfig) {

  return {
    getSubResource<T>(path: string, params?: Record<string, unknown>): Promise<ServerResult<T>> {
      return apiClient.get(`${config.basePath}/${path}`, { params }).then(res => res.data as ServerResult<T>)
    },

    postSubResource<T>(path: string, data?: unknown): Promise<ServerResult<T>> {
      return apiClient.post(`${config.basePath}/${path}`, data).then(res => res.data as ServerResult<T>)
    },

    putSubResource<T>(path: string, data?: unknown): Promise<ServerResult<T>> {
      return apiClient.put(`${config.basePath}/${path}`, data).then(res => res.data as ServerResult<T>)
    },

    deleteSubResource<T>(path: string, params?: Record<string, unknown>): Promise<ServerResult<T>> {
      return apiClient.delete(`${config.basePath}/${path}`, { params }).then(res => res.data as ServerResult<T>)
    },

    postAction<T>(path: string, data?: unknown): Promise<ServerResult<T>> {
      return apiClient.post(`${config.basePath}/${path}`, data).then(res => res.data as ServerResult<T>)
    },
  }
}
