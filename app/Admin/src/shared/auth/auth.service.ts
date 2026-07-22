import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import { TokenService } from './token.service'
import { STORAGE_KEYS } from '@/shared/constants'

interface LoginRequest {
  email: string
  password: string
}

interface LoginResponse {
  accessToken: string
  refreshToken: string
}

interface CurrentUser {
  id: string
  email: string
  name: string
  role: string
  permissions: string[]
}

export class AuthService {
  static async login(request: LoginRequest): Promise<Result<LoginResponse>> {
    const response = await apiClient.post<Result<LoginResponse>>(
      '/store/identity/auth/sessions/login',
      request
    )
    const result = response.data
    if (result.isSuccess) {
      TokenService.setTokens(result.value.accessToken, result.value.refreshToken)
    }
    return result
  }

  static async logout(): Promise<void> {
    const refreshToken = TokenService.getRefreshToken()
    if (refreshToken) {
      await apiClient.post('/store/identity/auth/sessions/logout', {
        refreshToken,
      }).catch(() => {})
    }
    TokenService.clearTokens()
    localStorage.removeItem(STORAGE_KEYS.USER)
  }

  static async getCurrentUser(): Promise<Result<CurrentUser>> {
    const response = await apiClient.get<Result<CurrentUser>>(
      '/store/identity/auth/sessions/me'
    )
    return response.data
  }

  static isAuthenticated(): boolean {
    return TokenService.hasValidAccessToken()
  }
}
