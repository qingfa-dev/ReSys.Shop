import type { InternalAxiosRequestConfig } from 'axios'
import { tokenService } from '../services/token.service'

export function authInterceptor(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig {
  const token = tokenService.getAccessToken()
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
}
