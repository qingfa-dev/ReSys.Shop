import type { TokenPair } from '../types'

const ACCESS_KEY = 'accessToken'
const REFRESH_KEY = 'refreshToken'
const ACCESS_EXPIRY_KEY = 'accessTokenExpiresAt'
const REFRESH_EXPIRY_KEY = 'refreshTokenExpiresAt'

export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_KEY)
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_KEY)
}

export function setTokens(pair: TokenPair): void {
  localStorage.setItem(ACCESS_KEY, pair.accessToken)
  localStorage.setItem(REFRESH_KEY, pair.refreshToken)
  const accessExpiresAt = Date.now() + pair.accessTokenExpiresIn * 1000
  const refreshExpiresAt = Date.now() + pair.refreshTokenExpiresIn * 1000
  localStorage.setItem(ACCESS_EXPIRY_KEY, String(accessExpiresAt))
  localStorage.setItem(REFRESH_EXPIRY_KEY, String(refreshExpiresAt))
}

export function clearTokens(): void {
  localStorage.removeItem(ACCESS_KEY)
  localStorage.removeItem(REFRESH_KEY)
  localStorage.removeItem(ACCESS_EXPIRY_KEY)
  localStorage.removeItem(REFRESH_EXPIRY_KEY)
}

export function hasValidAccessToken(): boolean {
  const token = getAccessToken()
  if (!token) return false
  const expiry = localStorage.getItem(ACCESS_EXPIRY_KEY)
  if (!expiry) return false
  return Date.now() < Number(expiry) - 30_000 // 30s buffer
}
