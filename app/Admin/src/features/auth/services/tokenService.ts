import { STORAGE_KEYS } from '@/shared/constants/storage'

interface TokenPair {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export function getAccessToken(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
  } catch {
    return null
  }
}

export function getRefreshToken(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)
  } catch {
    return null
  }
}

export function setTokens(pair: TokenPair): void {
  try {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, pair.accessToken)
    localStorage.setItem(`${STORAGE_KEYS.ACCESS_TOKEN}_expires_at`, String(pair.accessTokenExpiresIn))
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, pair.refreshToken)
    localStorage.setItem(`${STORAGE_KEYS.REFRESH_TOKEN}_expires_at`, String(pair.refreshTokenExpiresIn))
  } catch {
    // Ignore — localStorage may be unavailable
  }
}

export function clearTokens(): void {
  try {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
    localStorage.removeItem(`${STORAGE_KEYS.ACCESS_TOKEN}_expires_at`)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
    localStorage.removeItem(`${STORAGE_KEYS.REFRESH_TOKEN}_expires_at`)
  } catch {
    // Ignore
  }
}

export function hasValidAccessToken(): boolean {
  try {
    const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
    if (!token) return false
    const expiresAt = localStorage.getItem(`${STORAGE_KEYS.ACCESS_TOKEN}_expires_at`)
    if (!expiresAt) return true
    return Number(expiresAt) > Date.now() / 1000
  } catch {
    return false
  }
}
