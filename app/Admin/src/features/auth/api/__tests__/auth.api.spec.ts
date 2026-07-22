 
import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
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

function mockResult<T>(value: T): any {
  return { data: { isSuccess: true, statusCode: 200, value, errors: [], message: null, metadata: null } }
}

describe('auth.api', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('loginApi', () => {
    it('posts to login-password endpoint with credential and password', async () => {
      vi.mocked(apiClient.post).mockResolvedValue(mockResult({ accessToken: 'at', refreshToken: 'rt', accessTokenExpiresIn: 0, refreshTokenExpiresIn: 0 }))

      const result = await loginApi('user@test.com', 'secret')

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/login/password', { credential: 'user@test.com', password: 'secret' })
      expect(result.isSuccess).toBe(true)
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
    it('posts to logout endpoint with refresh token', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: null })

      await logoutApi('test-refresh-token')

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/logout', { refreshToken: 'test-refresh-token' })
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
