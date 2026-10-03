import type { InternalAxiosRequestConfig } from 'axios'

const TOKEN_KEY = 'accessToken'

export function requestInterceptor(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig {
  const token = localStorage.getItem(TOKEN_KEY)
  
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  
  return config
}
