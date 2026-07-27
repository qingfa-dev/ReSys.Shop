import { describe, it, expect, beforeEach } from 'vitest'
import { authInterceptor, setTokenGetter } from '../interceptors/auth'

function mockConfig(overrides: Record<string, unknown> = {}) {
  return {
    url: '/api/products',
    headers: {} as Record<string, string>,
    ...overrides,
  } as any
}

beforeEach(() => {
  localStorage.clear()
  setTokenGetter(() => localStorage.getItem('accessToken'))
})

describe('authInterceptor', () => {
  it('adds Authorization header when token exists', () => {
    localStorage.setItem('accessToken', 'jwt-token')
    const config = mockConfig()
    const result = authInterceptor(config)
    expect(result.headers.Authorization).toBe('Bearer jwt-token')
  })

  it('does not add header when no token', () => {
    const config = mockConfig()
    const result = authInterceptor(config)
    expect(result.headers.Authorization).toBeUndefined()
  })

  it('skips auth for login URLs', () => {
    localStorage.setItem('accessToken', 'jwt-token')
    const config = mockConfig({ url: '/api/identity/auth/login' })
    const result = authInterceptor(config)
    expect(result.headers.Authorization).toBeUndefined()
  })

  it('skips auth for refresh URLs', () => {
    localStorage.setItem('accessToken', 'jwt-token')
    const config = mockConfig({ url: '/api/identity/auth/sessions/refresh' })
    const result = authInterceptor(config)
    expect(result.headers.Authorization).toBeUndefined()
  })

  it('uses custom token getter', () => {
    setTokenGetter(() => 'custom-token')
    const config = mockConfig()
    const result = authInterceptor(config)
    expect(result.headers.Authorization).toBe('Bearer custom-token')
  })
})
