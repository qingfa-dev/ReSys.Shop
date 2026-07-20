import { describe, it, expect, beforeEach } from 'vitest'
import { tokenService } from '../token.service'

describe('tokenService', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('getAccessToken returns null when not set', () => {
    expect(tokenService.getAccessToken()).toBeNull()
  })

  it('getAccessToken returns value after setTokens', () => {
    tokenService.setTokens('access-123', 'refresh-456')
    expect(tokenService.getAccessToken()).toBe('access-123')
  })

  it('getRefreshToken returns value after setTokens', () => {
    tokenService.setTokens('access-123', 'refresh-456')
    expect(tokenService.getRefreshToken()).toBe('refresh-456')
  })

  it('clearTokens removes both tokens', () => {
    tokenService.setTokens('access-123', 'refresh-456')
    tokenService.clearTokens()
    expect(tokenService.getAccessToken()).toBeNull()
    expect(tokenService.getRefreshToken()).toBeNull()
  })

  it('hasTokens returns true only when both tokens are present', () => {
    expect(tokenService.hasTokens()).toBe(false)
    tokenService.setTokens('access-123', 'refresh-456')
    expect(tokenService.hasTokens()).toBe(true)
    tokenService.clearTokens()
    expect(tokenService.hasTokens()).toBe(false)
  })
})
