import apiClient from '../http/api.client'
import type { ServerResult } from '../types/result.types'

export interface ModuleApiConfig {
  basePath: string
}

export function createModuleApi<_T>(config: ModuleApiConfig) {

  return {
    async getSubResource<T>(path: string, params?: Record<string, unknown>): Promise<ServerResult<T>> {
      const res = await apiClient.get(`${config.basePath}/${path}`, { params })
      return res.data as ServerResult<T>
    },

    async postSubResource<T>(path: string, data?: unknown): Promise<ServerResult<T>> {
      const res = await apiClient.post(`${config.basePath}/${path}`, data)
      return res.data as ServerResult<T>
    },

    async putSubResource<T>(path: string, data?: unknown): Promise<ServerResult<T>> {
      const res = await apiClient.put(`${config.basePath}/${path}`, data)
      return res.data as ServerResult<T>
    },

    async deleteSubResource<T>(path: string, params?: Record<string, unknown>): Promise<ServerResult<T>> {
      const res = await apiClient.delete(`${config.basePath}/${path}`, { params })
      return res.data as ServerResult<T>
    },

    async postAction<T>(path: string, data?: unknown): Promise<ServerResult<T>> {
      const res = await apiClient.post(`${config.basePath}/${path}`, data)
      return res.data as ServerResult<T>
    },
  }
}
