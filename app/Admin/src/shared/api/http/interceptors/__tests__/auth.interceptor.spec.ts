import { describe, it, expect, beforeEach, vi } from 'vitest'
import { authInterceptor } from '../auth.interceptor'
import type { InternalAxiosRequestConfig } from 'axios'

describe('authInterceptor', () => {
  let config: InternalAxiosRequestConfig

  beforeEach(() => {
    vi.restoreAllMocks()
    localStorage.clear()
    config = { headers: {} } as InternalAxiosRequestConfig
  })

  it('adds Bearer token when accessToken exists', () => {
    localStorage.setItem('accessToken', 'test-token-123')
    const result = authInterceptor(config)
    expect(result.headers?.Authorization).toBe('Bearer test-token-123')
  })

  it('does not add header when no accessToken', () => {
    const result = authInterceptor(config)
    expect(result.headers?.Authorization).toBeUndefined()
  })

  it('does not modify headers config if headers is missing', () => {
    const noHeaders = {} as InternalAxiosRequestConfig
    const result = authInterceptor(noHeaders)
    expect(result).toBe(noHeaders)
  })

  it('returns the same config object', () => {
    localStorage.setItem('accessToken', 'token')
    const result = authInterceptor(config)
    expect(result).toBe(config)
  })
})
