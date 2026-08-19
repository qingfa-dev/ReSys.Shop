import type { InternalAxiosRequestConfig } from 'axios'
import { STORAGE_KEYS } from '@/shared/constants/storage'

// Initialize: Default token getter reads from localStorage (swallow SSR errors)
let _tokenGetter: () => string | null = () => {
  try {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
  } catch {
    return null
  }
}

// Assign: Override token getter — used for SSR or test isolation
export function setTokenGetter(getter: () => string | null) {
  _tokenGetter = getter
}

// Filter: Skip auth header for unauthenticated endpoints
const SKIP_AUTH_URLS = ['/sessions/refresh', '/sessions/login', '/auth/login']

// Intercept: Attach Bearer token to outgoing requests (skip auth URLs)
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
