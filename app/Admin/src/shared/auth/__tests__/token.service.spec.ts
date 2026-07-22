import { describe, it, expect, beforeEach } from 'vitest'
import { TokenService } from '../token.service'

describe('TokenService', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('should store and retrieve access token', () => {
    TokenService.setTokens('access-1', 'refresh-1')
    expect(TokenService.getAccessToken()).toBe('access-1')
    expect(TokenService.getRefreshToken()).toBe('refresh-1')
  })

  it('should clear tokens', () => {
    TokenService.setTokens('access-1', 'refresh-1')
    TokenService.clearTokens()
    expect(TokenService.getAccessToken()).toBeNull()
    expect(TokenService.getRefreshToken()).toBeNull()
  })

  it('should detect expired token', () => {
    expect(TokenService.hasValidAccessToken()).toBe(false)
  })
})
