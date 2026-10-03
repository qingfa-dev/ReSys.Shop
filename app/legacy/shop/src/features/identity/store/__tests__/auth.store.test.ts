import { beforeEach, describe, expect, it, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth'
import type { User, LoginRequest, RegisterRequest } from '../../types'

vi.mock('../../services/user/user.service', () => ({
  userService: {
    getProfile: vi.fn(),
    updateProfile: vi.fn(),
    changePassword: vi.fn(),
    requestPasswordReset: vi.fn(),
  },
}))

vi.mock('../../services/auth/auth.service', () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
  },
}))

import { userService } from '../../services/user/user.service'
import { authService } from '../../services/auth/auth.service'

describe('useAuthStore', () => {
  const mockUser = {
    id: 'user-1',
    email: 'test@example.com',
    firstName: 'John',
    lastName: 'Doe',
    phone: '+1234567890',
    avatar: '/avatar.jpg',
    role: 'customer' as const,
    emailVerified: true,
    mfaEnabled: false,
    createdAt: '2024-01-01',
    updatedAt: '2024-01-01',
  }

  const mockAuthResponse = {
    user: mockUser,
    tokens: {
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
      expiresIn: 3600,
    },
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    localStorage.clear()
  })

  describe('state', () => {
    it('should initialize with default values', () => {
      const store = useAuthStore()
      expect(store.user).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.initialized).toBe(false)
    })
  })

  describe('computed', () => {
    it('should compute isAuthenticated correctly', () => {
      const store = useAuthStore()
      expect(store.isAuthenticated).toBe(false)
      store.user = mockUser
      expect(store.isAuthenticated).toBe(true)
    })

    it('should compute isAdmin correctly', () => {
      const store = useAuthStore()
      expect(store.isAdmin).toBe(false)
      store.user = { ...mockUser, role: 'admin' }
      expect(store.isAdmin).toBe(true)
    })

    it('should compute fullName correctly', () => {
      const store = useAuthStore()
      expect(store.fullName).toBe('')
      store.user = mockUser
      expect(store.fullName).toBe('John Doe')
    })
  })

  describe('initialize', () => {
    it('should not fetch profile if already initialized', async () => {
      const store = useAuthStore()
      store.initialized = true
      await store.initialize()
      expect(userService.getProfile).not.toHaveBeenCalled()
    })

    it('should not fetch profile if no token', async () => {
      const store = useAuthStore()
      await store.initialize()
      expect(userService.getProfile).not.toHaveBeenCalled()
      expect(store.initialized).toBe(true)
    })

    it('should fetch profile if token exists', async () => {
      localStorage.setItem('accessToken', 'test-token')
      localStorage.setItem('userId', 'user-1')
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: mockUser }
      vi.mocked(userService.getProfile).mockResolvedValue(mockResult)

      const store = useAuthStore()
      await store.initialize()

      expect(userService.getProfile).toHaveBeenCalledWith('user-1')
      expect(store.user).toEqual(mockUser)
      expect(store.initialized).toBe(true)
    })

    it('should clear token on error', async () => {
      localStorage.setItem('accessToken', 'test-token')
      vi.mocked(userService.getProfile).mockRejectedValue(new Error('Network error'))

      const store = useAuthStore()
      await store.initialize()

      expect(localStorage.getItem('accessToken')).toBeNull()
      expect(store.initialized).toBe(true)
    })
  })

  describe('login', () => {
    it('should login successfully', async () => {
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: mockAuthResponse }
      vi.mocked(authService.login).mockResolvedValue(mockResult)

      const store = useAuthStore()
      await store.login({ email: 'test@example.com', password: 'password' })

      expect(store.user).toEqual(mockUser)
      expect(localStorage.getItem('accessToken')).toBe('access-token')
      expect(localStorage.getItem('userId')).toBe('user-1')
      expect(store.loading).toBe(false)
    })

    it('should throw error on failure', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 401, message: 'Invalid credentials', errors: [] }
      vi.mocked(authService.login).mockResolvedValue(mockResult)

      const store = useAuthStore()
      const fn = () => store.login({ email: 'test@example.com', password: 'wrong' })

      await expect(fn).rejects.toThrow()
      expect(store.error).toBe('Invalid credentials')
    })
  })

  describe('register', () => {
    it('should register successfully', async () => {
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 201, data: mockAuthResponse }
      vi.mocked(authService.register).mockResolvedValue(mockResult)

      const store = useAuthStore()
      await store.register({ email: 'test@example.com', password: 'password', firstName: 'John', lastName: 'Doe' })

      expect(store.user).toEqual(mockUser)
      expect(localStorage.getItem('accessToken')).toBe('access-token')
    })

    it('should throw error on failure', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 400, message: 'Email exists', errors: [] }
      vi.mocked(authService.register).mockResolvedValue(mockResult)

      const store = useAuthStore()
      const fn = () => store.register({ email: 'test@example.com', password: 'password', firstName: 'John', lastName: 'Doe' })

      await expect(fn).rejects.toThrow()
      expect(store.error).toBe('Email exists')
    })
  })

  describe('logout', () => {
    it('should logout and clear storage', async () => {
      localStorage.setItem('accessToken', 'test-token')
      vi.mocked(authService.logout).mockResolvedValue({ isSuccess: true, isFailure: false, statusCode: 200 })

      const store = useAuthStore()
      store.user = mockUser
      await store.logout()

      expect(store.user).toBeNull()
      expect(localStorage.getItem('accessToken')).toBeNull()
    })
  })

  describe('fetchProfile', () => {
    it('should fetch profile if token exists', async () => {
      localStorage.setItem('accessToken', 'test-token')
      localStorage.setItem('userId', 'user-1')
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: mockUser }
      vi.mocked(userService.getProfile).mockResolvedValue(mockResult)

      const store = useAuthStore()
      await store.fetchProfile()

      expect(store.user).toEqual(mockUser)
    })

    it('should not fetch if no token', async () => {
      const store = useAuthStore()
      await store.fetchProfile()

      expect(userService.getProfile).not.toHaveBeenCalled()
    })

    it('should set user to null on error', async () => {
      localStorage.setItem('accessToken', 'test-token')
      localStorage.setItem('userId', 'user-1')
      vi.mocked(userService.getProfile).mockRejectedValue(new Error('Error'))

      const store = useAuthStore()
      await store.fetchProfile()

      expect(store.user).toBeNull()
    })
  })

  describe('updateProfile', () => {
    it('should update profile successfully', async () => {
      const updatedUser = { ...mockUser, firstName: 'Jane' }
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: updatedUser }
      vi.mocked(userService.updateProfile).mockResolvedValue(mockResult)

      const store = useAuthStore()
      store.user = mockUser
      await store.updateProfile('user-1', { firstName: 'Jane' })

      expect(store.user).toEqual(updatedUser)
    })

    it('should throw error on failure', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 400, message: 'Update failed', errors: [] }
      vi.mocked(userService.updateProfile).mockResolvedValue(mockResult)

      const store = useAuthStore()
      const fn = () => store.updateProfile('user-1', { firstName: 'Jane' })

      await expect(fn).rejects.toThrow()
      expect(store.error).toBe('Update failed')
    })
  })
})