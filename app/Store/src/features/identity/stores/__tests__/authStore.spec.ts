import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createTestingPinia } from '@pinia/testing'
import { setActivePinia } from 'pinia'
import { ok, failure } from '@/shared/types/result'
import { useAuthStore } from '../authStore'
import * as authApi from '../../services/authApi'
import * as tokenService from '../../services/tokenService'
import * as cartApi from '../../../ordering/services/cartApi'
import type { TokenPair, SessionUser } from '../../types/auth'
import type { CartResponse } from '../../../ordering/types/cart'

const mockedAuthApi = vi.mocked(authApi)
const mockedTokenService = vi.mocked(tokenService)
const mockedCartApi = vi.mocked(cartApi)

vi.mock('@/features/identity/services/authApi', () => ({
  login: vi.fn<(...args: unknown[]) => unknown>(),
  getSession: vi.fn<(...args: unknown[]) => unknown>(),
  logout: vi.fn<(...args: unknown[]) => unknown>(),
  getLoginProviders: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/features/identity/services/tokenService', () => ({
  getAccessToken: vi.fn<(...args: unknown[]) => unknown>(),
  getRefreshToken: vi.fn<(...args: unknown[]) => unknown>(),
  setTokens: vi.fn<(...args: unknown[]) => unknown>(),
  clearTokens: vi.fn<(...args: unknown[]) => unknown>(),
  hasValidAccessToken: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/interceptors/auth', () => ({
  setTokenGetter: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/features/ordering/services/cartApi', () => ({
  getCart: vi.fn<(...args: unknown[]) => unknown>(),
  addItem: vi.fn<(...args: unknown[]) => unknown>(),
  updateItem: vi.fn<(...args: unknown[]) => unknown>(),
  removeItem: vi.fn<(...args: unknown[]) => unknown>(),
  emptyCart: vi.fn<(...args: unknown[]) => unknown>(),
  associateCart: vi.fn<(...args: unknown[]) => unknown>(),
}))

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

const cartResponse: CartResponse = {
  id: 'cart-1',
  items: [],
  itemCount: 0,
  subtotal: 0,
  currency: 'VND',
}

describe('authStore', () => {
  beforeEach(() => {
    setActivePinia(createTestingPinia({ stubActions: false, createSpy: vi.fn }))
    vi.clearAllMocks()
  })

  it('login success sets authenticated, hydrates user, and merges the cart', async () => {
    const store = useAuthStore()
    mockedAuthApi.login.mockResolvedValue(ok(tokenPair))
    mockedAuthApi.getSession.mockResolvedValue(ok(sessionUser))
    mockedCartApi.associateCart.mockResolvedValue(ok(cartResponse))
    mockedCartApi.getCart.mockResolvedValue(ok(cartResponse))

    const success = await store.login('u1@example.com', 'password')

    expect(success).toBe(true)
    expect(mockedTokenService.setTokens).toHaveBeenCalledWith(tokenPair)
    expect(store.status).toBe('authenticated')
    expect(store.user).toEqual({
      userId: 'u1',
      userName: 'User One',
      email: 'u1@example.com',
      roles: ['customer'],
      permissions: ['shop:view'],
      isAuthenticated: true,
    })
    expect(store.isAuthenticated).toBe(true)
    // Cart merge wiring from Task 6.4: associate + hydrate after login.
    expect(mockedCartApi.associateCart).toHaveBeenCalledTimes(1)
    expect(mockedCartApi.getCart).toHaveBeenCalledTimes(1)
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
    mockedTokenService.hasValidAccessToken.mockReturnValue(true)
    mockedAuthApi.getSession.mockResolvedValue(ok(sessionUser))

    await store.init()

    expect(store.status).toBe('authenticated')
    expect(store.user?.userId).toBe('u1')
    expect(store.user?.userName).toBe('User One')
    expect(store.isAuthenticated).toBe(true)
  })

  it('init stays idle when there is no valid token', async () => {
    const store = useAuthStore()
    mockedTokenService.hasValidAccessToken.mockReturnValue(false)

    await store.init()

    expect(store.status).toBe('idle')
    expect(store.user).toBeNull()
    expect(mockedAuthApi.getSession).not.toHaveBeenCalled()
  })

  it('init clears tokens when the session fetch fails', async () => {
    const store = useAuthStore()
    mockedTokenService.hasValidAccessToken.mockReturnValue(true)
    mockedAuthApi.getSession.mockResolvedValue(
      failure({ code: 'Session.Invalid', message: 'Session invalid', type: 401 }),
    )

    await store.init()

    expect(store.status).toBe('idle')
    expect(store.user).toBeNull()
    expect(mockedTokenService.clearTokens).toHaveBeenCalled()
  })

  it('init clears tokens when the session fetch throws', async () => {
    const store = useAuthStore()
    mockedTokenService.hasValidAccessToken.mockReturnValue(true)
    mockedAuthApi.getSession.mockRejectedValue(new Error('network down'))

    await store.init()

    expect(store.status).toBe('idle')
    expect(mockedTokenService.clearTokens).toHaveBeenCalled()
  })

  it('logout revokes tokens and clears state', async () => {
    const store = useAuthStore()
    store.user = {
      userId: 'u1',
      userName: 'User One',
      email: 'u1@example.com',
      roles: [],
      permissions: [],
      isAuthenticated: true,
    }
    store.status = 'authenticated'
    store.error = 'some error'
    mockedAuthApi.logout.mockResolvedValue(undefined)

    await store.logout()

    expect(mockedAuthApi.logout).toHaveBeenCalledWith({ revokeAll: undefined })
    expect(mockedTokenService.clearTokens).toHaveBeenCalled()
    expect(store.status).toBe('idle')
    expect(store.user).toBeNull()
    expect(store.error).toBeNull()
    expect(store.isAuthenticated).toBe(false)
  })
})
