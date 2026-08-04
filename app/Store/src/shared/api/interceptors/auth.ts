import type { InternalAxiosRequestConfig } from 'axios'
import { STORAGE_KEYS } from '@/shared/constants/storage'

let _tokenGetter: () => string | null = () => {
  try {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
  } catch {
    return null
  }
}

export function setTokenGetter(getter: () => string | null) {
  _tokenGetter = getter
}

const SKIP_AUTH_URLS = ['/sessions/refresh', '/sessions/login', '/auth/login']

export function authInterceptor(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig {
  if (SKIP_AUTH_URLS.some(url => config.url?.includes(url))) {
    return config
  }

  const token = _tokenGetter()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
}
