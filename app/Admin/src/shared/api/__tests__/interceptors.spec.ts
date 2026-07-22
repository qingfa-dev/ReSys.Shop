import { describe, it, expect, beforeEach, vi } from 'vitest'
import { authInterceptor } from '../interceptors/auth.interceptor'
import type { InternalAxiosRequestConfig } from 'axios'

describe('authInterceptor', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('should attach Authorization header when access token exists', () => {
    localStorage.setItem('accessToken', 'test-token-123')
    const config = { headers: {} } as InternalAxiosRequestConfig
    const result = authInterceptor(config)
    expect(result.headers?.['Authorization' as keyof typeof result.headers]).toBe('Bearer test-token-123')
  })

  it('should not attach Authorization header when no token', () => {
    const config = { headers: {} } as InternalAxiosRequestConfig
    const result = authInterceptor(config)
    expect(result.headers?.['Authorization' as keyof typeof result.headers]).toBeUndefined()
  })
})
