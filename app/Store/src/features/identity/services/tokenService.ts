import { STORAGE_KEYS } from '@/shared/constants/storage'
import type { TokenPair } from '../types'

const ACCESS_EXPIRY_KEY = 'accessTokenExpiresAt'
const REFRESH_EXPIRY_KEY = 'refreshTokenExpiresAt'

export function getAccessToken(): string | null {
  return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)
}

export function setTokens(pair: TokenPair): void {
  localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, pair.accessToken)
  localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, pair.refreshToken)
  const accessExpiresAt = Date.now() + pair.accessTokenExpiresIn * 1000
  const refreshExpiresAt = Date.now() + pair.refreshTokenExpiresIn * 1000
  localStorage.setItem(ACCESS_EXPIRY_KEY, String(accessExpiresAt))
  localStorage.setItem(REFRESH_EXPIRY_KEY, String(refreshExpiresAt))
}

export function clearTokens(): void {
  localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
  localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
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
