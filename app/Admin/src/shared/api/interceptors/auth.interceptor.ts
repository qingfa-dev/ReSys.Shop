import type { InternalAxiosRequestConfig } from 'axios'
import { STORAGE_KEYS } from '@/shared/constants'

export function authInterceptor(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig {
  const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
}
