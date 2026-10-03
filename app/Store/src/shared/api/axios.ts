import axios from 'axios'
import type { AxiosInstance } from 'axios'
import { STORAGE_KEYS } from '@/shared/constants/storage'
import { camelCaseInterceptor } from './interceptors/camelcase'
import { errorInterceptor } from './interceptors/error'

// State: Singleton Axios instance — lazily initialized via getApiClient()
let apiClient: AxiosInstance | null = null

// Create: Axios instance with JSON default header and Bearer token injection
export function createApiClient(baseURL?: string): AxiosInstance {
  apiClient = axios.create({
    baseURL: baseURL ?? '',
    headers: { 'Content-Type': 'application/json' },
  })

  // Intercept: Attach access token from localStorage to every request
  apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })

  // Intercept: Convert snake_case responses to camelCase and handle HTTP errors
  apiClient.interceptors.response.use(camelCaseInterceptor, errorInterceptor)

  return apiClient
}

// Acquire: Get or create the singleton Axios instance
export function getApiClient(): AxiosInstance {
  if (!apiClient) {
    apiClient = createApiClient()
  }
  return apiClient
}

// Dispose: Reset singleton — used in tests and logout flows
export function resetApiClient(): void {
  apiClient = null
}
