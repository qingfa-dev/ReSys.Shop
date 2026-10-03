import { jwtDecode } from 'jwt-decode'
import { STORAGE_KEYS } from '@/shared/constants'

interface JwtPayload {
  sub: string
  jti: string
  exp: number
  iat: number
  [key: string]: unknown
}

export class TokenService {
  static getAccessToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
  }

  static getRefreshToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)
  }

  static setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken)
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken)
  }

  static clearTokens(): void {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
  }

  static getAccessTokenPayload(): JwtPayload | null {
    const token = this.getAccessToken()
    if (!token) return null
    try {
      return jwtDecode<JwtPayload>(token)
    } catch {
      return null
    }
  }

  static isAccessTokenExpired(): boolean {
    const payload = this.getAccessTokenPayload()
    if (!payload) return true
    const now = Math.floor(Date.now() / 1000)
    return payload.exp < now
  }

  static hasValidAccessToken(): boolean {
    const token = this.getAccessToken()
    if (!token) return false
    return !this.isAccessTokenExpired()
  }
}
