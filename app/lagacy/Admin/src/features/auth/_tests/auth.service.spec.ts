import { describe, it, expect, vi, beforeEach } from 'vitest'
import { authService } from '../services/auth.service'
import apiClient from '@/shared/api/http/api.client'
import type { LoginRequest } from '../types/auth.types'

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
          accessToken: 'access',
          refreshToken: 'refresh',
          accessTokenExpiresAt: '2023-01-01',
          refreshTokenExpiresAt: '2023-01-02',
        },
        success: true,
      }

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse)

      const result = await authService.login(mockRequest)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/login/password', mockRequest)
      expect(result).toEqual(mockResponse)
    })
  })

  describe('refresh', () => {
    it('should call api.post with correct params', async () => {
      const mockRequest = { refreshToken: 'old-refresh' }
      const mockResponse = { data: { accessToken: 'new-access' }, success: true }

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse)

      const result = await authService.refresh(mockRequest)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/sessions/refresh', mockRequest)
      expect(result).toEqual(mockResponse)
    })
  })

  describe('logout', () => {
    it('should call api.post to logout endpoint', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: {}, success: true })

      await authService.logout()

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/logout', {})
    })
  })
})
