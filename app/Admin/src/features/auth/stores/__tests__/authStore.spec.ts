import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'

const { mockLogin, mockLogout, mockGetSession } = vi.hoisted(() => ({
  mockLogin: vi.fn(),
  mockLogout: vi.fn(),
  mockGetSession: vi.fn(),
}))

vi.mock('../../services/authApi', () => ({
  login: mockLogin,
  logout: mockLogout,
  getSession: mockGetSession,
}))

vi.mock('../../services/tokenService', () => ({
  getAccessToken: vi.fn(() => 'access-token'),
  getRefreshToken: vi.fn(() => 'refresh-token'),
  setTokens: vi.fn(),
  clearTokens: vi.fn(),
  hasValidAccessToken: vi.fn(() => true),
}))

import { useAuthStore } from '../authStore'

function makeSuccessResult<T>(value: T) {
  return { isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null, value }
}

function makeFailureResult(message: string) {
  return {
    isSuccess: false,
    statusCode: 401,
    message,
    errors: [{ code: 'Error', message, type: 401 }],
    metadata: null,
    value: null,
  }
}

beforeEach(() => {
  setActivePinia(createPinia())
  vi.clearAllMocks()
})

describe('authStore', () => {
  describe('initial state', () => {
    it('has idle status and null user', () => {
      const store = useAuthStore()
      expect(store.status).toBe('idle')
      expect(store.user).toBeNull()
      expect(store.error).toBeNull()
    })

    it('isAuthenticated is false', () => {
      const store = useAuthStore()
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('login', () => {
    it('sets status to authenticated on success', async () => {
      mockLogin.mockResolvedValue(makeSuccessResult({
        accessToken: 'at', accessTokenExpiresIn: 99, refreshToken: 'rt', refreshTokenExpiresIn: 88,
      }))
      mockGetSession.mockResolvedValue(makeSuccessResult({
        id: 'uid', roles: ['Admin'], permissions: ['read'],
      }))

      const store = useAuthStore()
      await store.login('admin', 'pass')

      expect(store.status).toBe('authenticated')
      expect(store.user?.userId).toBe('uid')
      expect(store.user?.roles).toEqual(['Admin'])
    })

    it('sets status to error on failure', async () => {
      mockLogin.mockResolvedValue(makeFailureResult('Invalid credentials'))

      const store = useAuthStore()
      await store.login('wrong', 'wrong')

      expect(store.status).toBe('error')
      expect(store.error).toBe('Invalid credentials')
    })
  })

  describe('logout', () => {
    it('resets state to idle', async () => {
      const store = useAuthStore()
      store.$patch({ user: { userId: 'x', userName: 'Test', email: 'test@test.com', roles: [], permissions: [], isAuthenticated: true }, status: 'authenticated' })

      mockLogout.mockResolvedValue({ isSuccess: true })

      await store.logout()

      expect(store.status).toBe('idle')
      expect(store.user).toBeNull()
    })

    it('sets isLoggingOut to true during logout and false after', async () => {
      const store = useAuthStore()

      store.user = { userId: 'u1', userName: 'User1', email: 'u1@test.com', roles: [], permissions: [], isAuthenticated: true }
      store.status = 'authenticated'

      const promise = store.logout()
      expect(store.isLoggingOut).toBe(true)

      await promise
      expect(store.isLoggingOut).toBe(false)
      expect(store.isAuthenticated).toBe(false)
    })

    it('sets isLoggingOut to false even when logout API fails', async () => {
      mockLogout.mockRejectedValueOnce(new Error('Network error'))
      const store = useAuthStore()
      store.user = { userId: 'u1', userName: 'User1', email: 'u1@test.com', roles: [], permissions: [], isAuthenticated: true }
      store.status = 'authenticated'

      await store.logout()
      expect(store.isLoggingOut).toBe(false)
    })
  })
})
