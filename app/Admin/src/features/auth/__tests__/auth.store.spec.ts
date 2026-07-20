import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../store/auth.store'
import { authRepository } from '../api/auth.api'

// Mock dependencies
import { createMockErrorResult } from '@/common/test/mock-types'
import type { AuthSession } from '../models/auth.model'

vi.mock('../api/auth.api', () => ({
  authRepository: {
    login: vi.fn<any>(),
    logout: vi.fn<any>(),
    refresh: vi.fn<any>(),
  },
}))

// Mock jwt-decode
vi.mock('jwt-decode', () => ({
  jwtDecode: vi.fn(() => ({ sub: 'user-123', name: 'Test User' })),
}))

// Mock router
const mockRouterPush = vi.fn<any>()
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: mockRouterPush,
  }),
}))

describe('AuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    localStorage.clear()
  })

  describe('Initialization', () => {
    it('should initialize with tokens from localStorage', () => {
      localStorage.setItem('accessToken', 'stored-access')
      localStorage.setItem('refreshToken', 'stored-refresh')

      const store = useAuthStore()

      expect(store.accessToken).toBe('stored-access')
      expect(store.refreshToken).toBe('stored-refresh')
      expect(store.isAuthenticated).toBe(true)
    })

    it('should be unauthenticated if no tokens in localStorage', () => {
      const store = useAuthStore()

      expect(store.accessToken).toBeNull()
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('Getters', () => {
    it('user should return decoded token data', () => {
      localStorage.setItem('accessToken', 'valid-token')
      const store = useAuthStore()

      expect(store.user).toEqual({ sub: 'user-123', name: 'Test User' })
    })

    it('user should return null if no token', () => {
      const store = useAuthStore()
      expect(store.user).toBeNull()
    })
  })

  describe('Actions', () => {
    it('login should call service and set tokens', async () => {
      const store = useAuthStore()
      const mockLoginResponse = {
        isSuccess: true,
        statusCode: 200,
        errors: [],
        message: null,
        metadata: null,
        value: {
          accessToken: 'new-access',
          accessTokenExpiresIn: 3600,
          refreshToken: 'new-refresh',
          refreshTokenExpiresIn: 86400,
          user: null,
        },
      }

      vi.mocked(authRepository.login).mockResolvedValue(mockLoginResponse)

      await store.login({ credential: 'user', password: 'pwd', rememberMe: false })

      expect(authRepository.login).toHaveBeenCalledWith({ credential: 'user', password: 'pwd', rememberMe: false })
      expect(store.accessToken).toBe('new-access')
      expect(store.refreshToken).toBe('new-refresh')
      expect(localStorage.getItem('accessToken')).toBe('new-access')
    })

    it('logout should call service and clear tokens', async () => {
      localStorage.setItem('accessToken', 'token')
      const store = useAuthStore()

      vi.mocked(authRepository.logout).mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined })

      await store.logout()

      expect(authRepository.logout).toHaveBeenCalled()
      expect(store.accessToken).toBeNull()
      expect(localStorage.getItem('accessToken')).toBeNull()
    })

    it('login should handle failure correctly', async () => {
      const store = useAuthStore()
      const mockErrorResponse = createMockErrorResult<AuthSession>({
        statusCode: 400,
        errors: [{ code: 'invalid_credentials', message: 'Invalid credentials', type: 0, metadata: null }],
        message: 'Invalid credentials',
      })

      vi.mocked(authRepository.login).mockResolvedValue(mockErrorResponse)

      const result = await store.login({ credential: 'user', password: 'bad-pwd', rememberMe: false })

      expect(store.accessToken).toBeNull()
      expect(result.isSuccess).toBe(false)
    })

    it('logout should clear tokens even if API fails', async () => {
      localStorage.setItem('accessToken', 'token')
      const store = useAuthStore()

      vi.mocked(authRepository.logout).mockRejectedValue(new Error('Network error'))

      await store.logout()

      expect(authRepository.logout).toHaveBeenCalled()
      expect(store.accessToken).toBeNull()
      expect(localStorage.getItem('accessToken')).toBeNull()
    })

    it('refreshSession should update tokens on success', async () => {
      localStorage.setItem('refreshToken', 'old-refresh')
      const store = useAuthStore()
      const mockRefreshResponse = {
        isSuccess: true,
        statusCode: 200,
        errors: [],
        message: null,
        metadata: null,
        value: {
          accessToken: 'fresh-access',
          accessTokenExpiresIn: 3600,
          refreshToken: 'fresh-refresh',
          refreshTokenExpiresIn: 86400,
          user: null,
        },
      }
 
      vi.mocked(authRepository.refresh).mockResolvedValue(mockRefreshResponse)

      const result = await store.refreshSession()

      expect(authRepository.refresh).toHaveBeenCalledWith({ refreshToken: 'old-refresh' })
      expect(store.accessToken).toBe('fresh-access')
      expect(result).toBe('fresh-access')
    })

    it('refreshSession should clear tokens on failure', async () => {
      localStorage.setItem('refreshToken', 'bad-refresh')
      const store = useAuthStore()

      vi.mocked(authRepository.refresh).mockRejectedValue(new Error('Invalid token'))

      const result = await store.refreshSession()

      expect(store.accessToken).toBeNull()
      expect(localStorage.getItem('refreshToken')).toBeNull()
      expect(result).toBeNull()
    })
  })
})
