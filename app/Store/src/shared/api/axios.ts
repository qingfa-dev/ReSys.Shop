import axios from 'axios'
import type { AxiosInstance } from 'axios'
import { STORAGE_KEYS } from '@/shared/constants/storage'

let apiClient: AxiosInstance | null = null

export function createApiClient(baseURL?: string): AxiosInstance {
  apiClient = axios.create({
    baseURL: baseURL ?? '',
    headers: { 'Content-Type': 'application/json' },
  })

  apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })

  return apiClient
}

export function getApiClient(): AxiosInstance {
  if (!apiClient) {
    apiClient = createApiClient()
  }
  return apiClient
}

export function resetApiClient(): void {
  apiClient = null
}
