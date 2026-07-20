import { describe, it, expect, vi } from 'vitest'
import type { InternalAxiosRequestConfig } from 'axios'

const tokenService = {
  getAccessToken: vi.fn(),
  getRefreshToken: vi.fn(),
  setTokens: vi.fn(),
  clearTokens: vi.fn(),
  hasTokens: vi.fn(),
}

vi.mock('@/common/auth/token.service', () => ({
  tokenService,
}))

describe('authInterceptor', () => {
  it('adds Bearer token when access token is set', async () => {
    tokenService.getAccessToken.mockReturnValue('my-token')
    const { authInterceptor } = await import('../auth.interceptor')

    const config = { headers: {} } as InternalAxiosRequestConfig
    const result = authInterceptor(config)

    expect(result.headers.Authorization).toBe('Bearer my-token')
  })

  it('does not add header when no token', async () => {
    tokenService.getAccessToken.mockReturnValue(null)
    const { authInterceptor } = await import('../auth.interceptor')

    const config = { headers: {} } as InternalAxiosRequestConfig
    const result = authInterceptor(config)

    expect(result.headers.Authorization).toBeUndefined()
  })

  it('gracefully handles null headers', async () => {
    tokenService.getAccessToken.mockReturnValue('my-token')
    const { authInterceptor } = await import('../auth.interceptor')

    const config = { headers: null } as unknown as InternalAxiosRequestConfig
    authInterceptor(config)

    expect(true).toBe(true)
  })
})
