import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet } = vi.hoisted(() => ({
  mockPost: vi.fn(),
  mockGet: vi.fn(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
}))

import { login, logout, getSession, forgotPassword, resetPassword } from '../authApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('login', () => {
  it('calls POST login/password with correct body', async () => {
    mockPost.mockResolvedValue({
      value: { accessToken: 'at', accessTokenExpiresIn: 999, refreshToken: 'rt', refreshTokenExpiresIn: 888 },
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    const result = await login({ credential: 'admin', password: 'pass' })

    expect(mockPost).toHaveBeenCalledWith('api/store/identity/auth/login/password', {
      credential: 'admin',
      password: 'pass',
    })
    expect(result.isSuccess).toBe(true)
    expect(result.value.accessToken).toBe('at')
  })

  it('returns failure when backend returns error', async () => {
    mockPost.mockResolvedValue({
      isSuccess: false,
      statusCode: 401,
      message: 'Invalid credentials',
      errors: [{ code: 'AuthFailed', message: 'Invalid credentials', type: 401 }],
      metadata: null,
      value: null,
    })

    const result = await login({ credential: 'wrong', password: 'wrong' })
    expect(result.isSuccess).toBe(false)
  })
})

describe('logout', () => {
  it('calls POST logout with body', async () => {
    mockPost.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: null,
    })

    await logout({ revokeAll: true })
    expect(mockPost).toHaveBeenCalledWith('api/store/identity/auth/logout', { revokeAll: true })
  })

  it('calls POST logout with empty body when no args', async () => {
    mockPost.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: null,
    })

    await logout()
    expect(mockPost).toHaveBeenCalledWith('api/store/identity/auth/logout', undefined)
  })
})

describe('getSession', () => {
  it('calls GET sessions', async () => {
    mockGet.mockResolvedValue({
      value: { id: 'uid-1', roles: ['Admin'], permissions: ['read'] },
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    const result = await getSession()
    expect(mockGet).toHaveBeenCalledWith('api/store/identity/auth/sessions')
    expect(result.isSuccess).toBe(true)
    expect(result.value.roles).toEqual(['Admin'])
  })
})

describe('forgotPassword', () => {
  it('calls POST passwords/forgot with email', async () => {
    mockPost.mockResolvedValue(undefined)

    await forgotPassword({ email: 'user@example.com' })
    expect(mockPost).toHaveBeenCalledWith('api/store/identity/passwords/forgot', {
      email: 'user@example.com',
    })
  })
})

describe('resetPassword', () => {
  it('calls POST passwords/reset with full body', async () => {
    mockPost.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: null,
    })

    await resetPassword({
      email: 'u@e.com',
      userId: 'uid',
      token: 'tok',
      newPassword: 'newpass123',
    })

    expect(mockPost).toHaveBeenCalledWith('api/store/identity/passwords/reset', {
      email: 'u@e.com',
      userId: 'uid',
      token: 'tok',
      newPassword: 'newpass123',
    })
  })
})
