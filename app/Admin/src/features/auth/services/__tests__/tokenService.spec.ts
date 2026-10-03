import { describe, it, expect, beforeEach } from 'vitest'
import {
  getAccessToken,
  getRefreshToken,
  setTokens,
  clearTokens,
  hasValidAccessToken,
} from '../tokenService'

function makePair(overrides: Partial<{ accessTokenExpiresIn: number; refreshTokenExpiresIn: number }> = {}) {
  return {
    accessToken: 'access-token-123',
    accessTokenExpiresIn: overrides.accessTokenExpiresIn ?? Date.now() / 1000 + 3600,
    refreshToken: 'refresh-token-456',
    refreshTokenExpiresIn: overrides.refreshTokenExpiresIn ?? Date.now() / 1000 + 7200,
  }
}

beforeEach(() => {
  localStorage.clear()
})

describe('getAccessToken', () => {
  it('returns null when no token stored', () => {
    expect(getAccessToken()).toBeNull()
  })

  it('returns token after setTokens', () => {
    setTokens(makePair())
    expect(getAccessToken()).toBe('access-token-123')
  })
})

describe('getRefreshToken', () => {
  it('returns null when no token stored', () => {
    expect(getRefreshToken()).toBeNull()
  })

  it('returns token after setTokens', () => {
    setTokens(makePair())
    expect(getRefreshToken()).toBe('refresh-token-456')
  })
})

describe('setTokens', () => {
  it('stores access and refresh tokens in localStorage', () => {
    setTokens(makePair())
    expect(localStorage.getItem('accessToken')).toBe('access-token-123')
    expect(localStorage.getItem('refreshToken')).toBe('refresh-token-456')
  })

  it('stores expiry timestamps', () => {
    setTokens(makePair())
    expect(localStorage.getItem('accessToken_expires_at')).toBeTruthy()
    expect(localStorage.getItem('refreshToken_expires_at')).toBeTruthy()
  })
})

describe('clearTokens', () => {
  it('removes all four localStorage keys', () => {
    setTokens(makePair())
    clearTokens()
    expect(localStorage.getItem('accessToken')).toBeNull()
    expect(localStorage.getItem('refreshToken')).toBeNull()
    expect(localStorage.getItem('accessToken_expires_at')).toBeNull()
    expect(localStorage.getItem('refreshToken_expires_at')).toBeNull()
  })
})

describe('hasValidAccessToken', () => {
  it('returns false when no token', () => {
    expect(hasValidAccessToken()).toBe(false)
  })

  it('returns true for a future expiry', () => {
    setTokens(makePair({ accessTokenExpiresIn: Date.now() / 1000 + 600 }))
    expect(hasValidAccessToken()).toBe(true)
  })

  it('returns false for a past expiry', () => {
    setTokens(makePair({ accessTokenExpiresIn: Date.now() / 1000 - 600 }))
    expect(hasValidAccessToken()).toBe(false)
  })
})
