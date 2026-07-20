import type { InternalAxiosRequestConfig } from 'axios'
import { tokenService } from '@/common/auth/token.service'

export function authInterceptor(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig {
  const token = tokenService.getAccessToken()
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
}
