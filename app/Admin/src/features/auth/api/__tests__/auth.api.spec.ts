/* eslint-disable @typescript-eslint/no-explicit-any */
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
    post: vi.fn<(...args: any[]) => any>(),
    get: vi.fn<(...args: any[]) => any>(),
  },
}))

vi.mock('@/shared/auth/auth.service', () => ({
  AuthService: {
    login: vi.fn<(...args: any[]) => any>(),
    logout: vi.fn<(...args: any[]) => any>(),
    getCurrentUser: vi.fn<(...args: any[]) => any>(),
    isAuthenticated: vi.fn<(...args: any[]) => any>(),
  },
}))

function mockResult<T>(value: T): any {
  return { data: { isSuccess: true, statusCode: 200, value, errors: [], message: null, metadata: null } }
}

describe('auth.api', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('loginApi', () => {
    it('calls AuthService.login with credential and password', async () => {
      const mockResponse = { isSuccess: true, statusCode: 200, value: { accessToken: 'at', refreshToken: 'rt' }, errors: [], message: null, metadata: null }
      vi.mocked(AuthService.login).mockResolvedValue(mockResponse as any)

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
    it('gets session from sessions endpoint', async () => {
      vi.mocked(apiClient.get).mockResolvedValue(mockResult({ id: '1', roles: ['admin'], permissions: ['*'] }))

      const result = await getSessionApi()

      expect(apiClient.get).toHaveBeenCalledWith('/store/identity/auth/sessions')
      expect(result.isSuccess).toBe(true)
    })
  })
})
