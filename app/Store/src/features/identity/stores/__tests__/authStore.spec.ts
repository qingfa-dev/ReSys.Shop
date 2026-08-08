import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createTestingPinia } from '@pinia/testing'
import { setActivePinia } from 'pinia'
import { useAuthStore } from '../authStore'
import { AuthApi } from '../../services/authApi'
import * as tokenService from '../../services/tokenService'
import type { TokenPair, SessionUser } from '../../types/auth'

vi.mock('../../services/authApi', () => ({
  AuthApi: {
    login: vi.fn<() => Promise<void>>(),
    getSession: vi.fn<() => Promise<void>>(),
    logout: vi.fn<() => Promise<void>>(),
    getLoginProviders: vi.fn<() => Promise<void>>(),
    register: vi.fn<() => Promise<void>>(),
    forgotPassword: vi.fn<() => Promise<void>>(),
    resetPassword: vi.fn<() => Promise<void>>(),
    changePassword: vi.fn<() => Promise<void>>(),
  },
}))

vi.mock('../../services/tokenService', () => ({
  getAccessToken: vi.fn<() => string | null>(),
  getRefreshToken: vi.fn<() => string | null>(),
  setTokens: vi.fn<() => void>(),
  clearTokens: vi.fn<() => void>(),
  hasValidAccessToken: vi.fn<() => boolean>(),
}))

vi.mock('@/shared/composables/useStoreEvents', () => ({
  emit: vi.fn<() => void>(),
  on: vi.fn<() => void>(),
  off: vi.fn<() => void>(),
}))

const mockedAuthApi = vi.mocked(AuthApi)
const mockedTokenService = vi.mocked(tokenService)

const tokenPair: TokenPair = {
  accessToken: 'header.payload.signature',
  accessTokenExpiresIn: 3600,
  refreshToken: 'refresh-token',
  refreshTokenExpiresIn: 86400,
}

const sessionUser: SessionUser = {
  id: 'u1',
  userName: 'User One',
  email: 'u1@example.com',
  roles: ['customer'],
  permissions: ['shop:view'],
}

function ok<T>(value: T) {
  return { isSuccess: true, statusCode: 200, message: null, errors: [], value }
}

function failure(error: { code: string; message: string; type: number }) {
  return { isSuccess: false, statusCode: error.type, message: error.message, errors: [error], value: undefined as never }
}

describe('authStore', () => {
  beforeEach(() => {
    setActivePinia(createTestingPinia({ stubActions: false, createSpy: vi.fn }))
    vi.clearAllMocks()
  })

  it('login success sets authenticated and hydrates user', async () => {
    const store = useAuthStore()
    mockedAuthApi.login.mockResolvedValue(ok(tokenPair))
    mockedAuthApi.getSession.mockResolvedValue(ok(sessionUser))

    const success = await store.login('u1@example.com', 'password')

    expect(success).toBe(true)
    expect(mockedTokenService.setTokens).toHaveBeenCalledWith(tokenPair)
    expect(store.status).toBe('authenticated')
    expect(store.user).toEqual({
      userId: sessionUser.id,
      userName: sessionUser.userName,
      email: sessionUser.email,
      roles: sessionUser.roles,
      permissions: sessionUser.permissions,
      isAuthenticated: true,
    })
    expect(store.isAuthenticated).toBe(true)
  })

  it('login failure sets error and stays unauthenticated', async () => {
    const store = useAuthStore()
    mockedAuthApi.login.mockResolvedValue(
      failure({ code: 'Auth.InvalidCredentials', message: 'Invalid credentials', type: 401 }),
    )

    const success = await store.login('u1@example.com', 'wrong')

    expect(success).toBe(false)
    expect(store.status).toBe('error')
    expect(store.error).toBe('Invalid credentials')
    expect(store.isAuthenticated).toBe(false)
    expect(store.user).toBeNull()
    expect(mockedTokenService.setTokens).not.toHaveBeenCalled()
  })

  it('init hydrates the user when a valid token exists', async () => {
    const store = useAuthStore()
    mockedTokenService.getAccessToken.mockReturnValue('some-token')
    mockedAuthApi.getSession.mockResolvedValue(ok(sessionUser))

    await store.init()

    expect(store.status).toBe('authenticated')
    expect(store.user?.userId).toBe('u1')
    expect(store.user?.userName).toBe('User One')
    expect(store.isAuthenticated).toBe(true)
  })

  it('init stays idle when there is no valid token', async () => {
    const store = useAuthStore()
    mockedTokenService.getAccessToken.mockReturnValue(null)

    await store.init()

    expect(store.status).toBe('idle')
    expect(store.user).toBeNull()
    expect(mockedAuthApi.getSession).not.toHaveBeenCalled()
  })

  it('logout revokes tokens and clears state', async () => {
    const store = useAuthStore()
    store.user = {
      userId: sessionUser.id,
      userName: sessionUser.userName,
      email: sessionUser.email,
      roles: sessionUser.roles,
      permissions: sessionUser.permissions,
      isAuthenticated: true,
    }
    store.status = 'authenticated'
    mockedAuthApi.logout.mockResolvedValue(ok(undefined as never))

    await store.logout()

    expect(mockedAuthApi.logout).toHaveBeenCalledWith({ revokeAll: false })
    expect(mockedTokenService.clearTokens).toHaveBeenCalled()
    expect(store.status).toBe('idle')
    expect(store.user).toBeNull()
    expect(store.isAuthenticated).toBe(false)
  })
})
