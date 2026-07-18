import { describe, it, expect, vi, beforeEach } from 'vitest'
import { authService } from '../services/auth.service'
import apiClient from '@/shared/api/http/api.client'
import type { LoginRequest } from '../types/Login.Request.Type'
import { createMockResult } from '@/shared/test/mock-types'
import type { AuthenticationResponse } from '../types/Login.Response.Type'
import type { AxiosResponse } from 'axios'

// Mock apiClient
vi.mock('@/shared/api/http/api.client', () => ({
  default: {
    post: vi.fn(),
  },
}))

describe('AuthService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('login', () => {
    it('should call api.post with correct params and return data', async () => {
      const mockRequest: LoginRequest = { credential: 'user', password: 'password', rememberMe: false }
      const mockResponse = {
        data: createMockResult<AuthenticationResponse>({
          accessToken: 'access',
          accessTokenExpiresIn: 3600,
          refreshToken: 'refresh',
          refreshTokenExpiresIn: 86400,
        }),
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {},
      } as AxiosResponse

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse)

      const result = await authService.login(mockRequest)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/login/password', mockRequest)
      expect(result.isSuccess).toBe(true)
      expect(result.value).toBeDefined()
    })
  })

  describe('refresh', () => {
    it('should call api.post with correct params', async () => {
      const mockRequest = { refreshToken: 'old-refresh' }
      const mockResponse = {
        data: createMockResult<AuthenticationResponse>({
          accessToken: 'new-access',
          accessTokenExpiresIn: 3600,
          refreshToken: 'new-refresh',
          refreshTokenExpiresIn: 86400,
        }),
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {},
      } as AxiosResponse

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse)

      const result = await authService.refresh(mockRequest)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/sessions/refresh', mockRequest)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('logout', () => {
    it('should call api.post to logout endpoint', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({
        data: createMockResult(undefined),
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {},
      } as AxiosResponse)

      await authService.logout()

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/logout', {})
    })
  })
})
