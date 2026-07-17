import { describe, it, expect, vi, beforeEach } from 'vitest'
import { authService } from '../services/auth.service'
import apiClient from '@/shared/api/http/api.client'
import type { LoginRequest } from '../types/auth.request.types'

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
      const mockRequest: LoginRequest = { credential: 'user', password: 'password' }
      const mockResponse = {
        data: {
          isSuccess: true,
          statusCode: 200,
          errors: [],
          message: null,
          metadata: null,
          value: {
            access_token: 'access',
            access_token_expires_in: 3600,
            refresh_token: 'refresh',
            refresh_token_expires_in: 86400,
          },
        },
      }

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse as any)

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
        data: {
          isSuccess: true,
          statusCode: 200,
          errors: [],
          message: null,
          metadata: null,
          value: { access_token: 'new-access', access_token_expires_in: 3600, refresh_token: 'new-refresh', refresh_token_expires_in: 86400 },
        },
      }

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse as any)

      const result = await authService.refresh(mockRequest)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/sessions/refresh', mockRequest)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('logout', () => {
    it('should call api.post to logout endpoint', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({
        data: { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined },
      } as any)

      await authService.logout()

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/logout', {})
    })
  })
})
