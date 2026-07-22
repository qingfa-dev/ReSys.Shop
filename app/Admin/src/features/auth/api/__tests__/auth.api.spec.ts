import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { AuthService } from '@/shared/auth/auth.service'
import {
  loginApi,
  registerApi,
  forgotPasswordApi,
  resetPasswordApi,
  changePasswordApi,
  logoutApi,
  getSessionApi,
} from '../auth.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    post: vi.fn<(...args: unknown[]) => unknown>(),
    get: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

vi.mock('@/shared/auth/auth.service', () => ({
  AuthService: {
    login: vi.fn<(...args: unknown[]) => unknown>(),
    logout: vi.fn<(...args: unknown[]) => unknown>(),
    getCurrentUser: vi.fn<(...args: unknown[]) => unknown>(),
    isAuthenticated: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

function mockResult<T>(value: T) {
  return { data: { isSuccess: true, statusCode: 200, value, errors: [], message: null } }
}

describe('auth.api', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('loginApi', () => {
    it('calls AuthService.login with credential and password', async () => {
      const mockResponse = { isSuccess: true, statusCode: 200, value: { accessToken: 'at', refreshToken: 'rt' }, errors: [], message: null }
      vi.mocked(AuthService.login).mockResolvedValue(mockResponse)

      const result = await loginApi('user@test.com', 'secret')

      expect(AuthService.login).toHaveBeenCalledWith({ email: 'user@test.com', password: 'secret' })
      expect(result).toBe(mockResponse)
    })
  })

  describe('registerApi', () => {
    it('posts to register endpoint', async () => {
      const fields = { email: 'a@b.com', userName: 'test', password: 'Pass1234!', firstName: 'Test', acceptTerm: true }
      vi.mocked(apiClient.post).mockResolvedValue(mockResult({ userId: '1', email: 'a@b.com', message: 'ok' }))

      const result = await registerApi(fields)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/register', fields)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('forgotPasswordApi', () => {
    it('posts to forgot-password endpoint', async () => {
      vi.mocked(apiClient.post).mockResolvedValue(mockResult(null))

      await forgotPasswordApi('a@b.com')

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/passwords/forgot', { email: 'a@b.com' })
    })
  })

  describe('resetPasswordApi', () => {
    it('posts to reset-password endpoint', async () => {
      const params = { email: 'a@b.com', userId: '1', token: 'tok', newPassword: 'Pass1234!' }
      vi.mocked(apiClient.post).mockResolvedValue(mockResult(null))

      await resetPasswordApi(params)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/passwords/reset', params)
    })
  })

  describe('changePasswordApi', () => {
    it('posts to change-password endpoint', async () => {
      const params = { email: 'a@b.com', currentPassword: 'old', newPassword: 'newPass1!' }
      vi.mocked(apiClient.post).mockResolvedValue(mockResult(null))

      await changePasswordApi(params)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/passwords/change', params)
    })
  })

  describe('logoutApi', () => {
    it('calls AuthService.logout', async () => {
      vi.mocked(AuthService.logout).mockResolvedValue(undefined)

      await logoutApi()

      expect(AuthService.logout).toHaveBeenCalled()
    })
  })

  describe('getSessionApi', () => {
    it('calls AuthService.getCurrentUser', async () => {
      const mockResponse = { isSuccess: true, statusCode: 200, value: { id: '1', email: 'a@b.com', name: 'A', role: 'admin', permissions: [] }, errors: [], message: null }
      vi.mocked(AuthService.getCurrentUser).mockResolvedValue(mockResponse as never)

      const result = await getSessionApi()

      expect(AuthService.getCurrentUser).toHaveBeenCalled()
      expect(result).toBe(mockResponse)
    })
  })
})
