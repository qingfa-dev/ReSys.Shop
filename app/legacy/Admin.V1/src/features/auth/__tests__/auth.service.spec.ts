import { describe, it, expect, vi, beforeEach } from 'vitest'
import { authRepository } from '../api/auth.api'
import apiClient from '@/common/api/http/api.client'
import type { LoginRequest } from '../types/login.request'
import { createMockResult } from '@/common/test/mock-types'
import type { LoginResponse } from '../types/login.response'
import type { AxiosResponse } from 'axios'

vi.mock('@/common/api/http/api.client', () => ({
  default: {
    post: vi.fn<any>(),
    get: vi.fn<any>(),
  },
}))

describe('AuthService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('login', () => {
    it('should call api.post with correct params and return data', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({
        data: createMockResult({ id: 'uid-1', email: 'a@b.com', fullName: 'John', roles: ['Admin'] }),
      } as AxiosResponse)

      const mockRequest: LoginRequest = { credential: 'user', password: 'password', rememberMe: false }
      const mockResponse = {
        data: createMockResult<LoginResponse>({
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

      const result = await authRepository.login(mockRequest)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/login/password', mockRequest)
      expect(result.isSuccess).toBe(true)
      expect(result.value).toBeDefined()
    })
  })

  describe('refresh', () => {
    it('should call api.post with correct params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({
        data: createMockResult({ id: 'uid-1', email: 'a@b.com', fullName: 'John', roles: ['Admin'] }),
      } as AxiosResponse)

      const mockRequest = { refreshToken: 'old-refresh' }
      const mockResponse = {
        data: createMockResult<LoginResponse>({
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

      const result = await authRepository.refresh(mockRequest)

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

      await authRepository.logout()

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/logout', {})
    })
  })
})
