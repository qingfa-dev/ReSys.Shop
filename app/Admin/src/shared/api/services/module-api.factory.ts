import apiClient from '../client'
import type { Result } from '@/shared/models'

export interface ModuleApiConfig {
  basePath: string
}

export function createModuleApi<_T>(config: ModuleApiConfig) {
  return {
    async getSubResource<T>(path: string, params?: Record<string, unknown>): Promise<Result<T>> {
      const res = await apiClient.get(`${config.basePath}/${path}`, { params })
      return res.data as Result<T>
    },

    async postSubResource<T>(path: string, data?: unknown): Promise<Result<T>> {
      const res = await apiClient.post(`${config.basePath}/${path}`, data)
      return res.data as Result<T>
    },

    async putSubResource<T>(path: string, data?: unknown): Promise<Result<T>> {
      const res = await apiClient.put(`${config.basePath}/${path}`, data)
      return res.data as Result<T>
    },

    async deleteSubResource<T>(path: string, params?: Record<string, unknown>): Promise<Result<T>> {
      const res = await apiClient.delete(`${config.basePath}/${path}`, { params })
      return res.data as Result<T>
    },

    async postAction<T>(path: string, data?: unknown): Promise<Result<T>> {
      const res = await apiClient.post(`${config.basePath}/${path}`, data)
      return res.data as Result<T>
    },
  }
}
