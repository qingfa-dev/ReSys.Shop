import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type {
  RegisterRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
  TokenResponse,
  RegisterResponse,
  SessionResponse,
} from '../types'

export class AuthApi {
  static async login(
    credential: string,
    password: string,
  ): Promise<Result<TokenResponse>> {
    const response = await apiClient.post<Result<TokenResponse>>(
      '/store/identity/auth/login/password',
      { credential, password },
    )
    return response.data
  }

  static async register(fields: RegisterRequest): Promise<Result<RegisterResponse>> {
    const response = await apiClient.post<Result<RegisterResponse>>(
      '/store/identity/auth/register',
      fields,
    )
    return response.data
  }

  static async forgotPassword(email: string): Promise<Result<null>> {
    const response = await apiClient.post<Result<null>>('/store/identity/passwords/forgot', {
      email,
    })
    return response.data
  }

  static async resetPassword(params: ResetPasswordRequest): Promise<Result<null>> {
    const response = await apiClient.post<Result<null>>('/store/identity/passwords/reset', params)
    return response.data
  }

  static async changePassword(params: ChangePasswordRequest): Promise<Result<null>> {
    const response = await apiClient.post<Result<null>>('/store/identity/passwords/change', params)
    return response.data
  }

  static async logout(refreshToken: string): Promise<void> {
    await apiClient.post('/store/identity/auth/logout', { refreshToken }).catch(() => {})
  }

  static async getSession(): Promise<Result<SessionResponse>> {
    const response = await apiClient.get<Result<SessionResponse>>('/store/identity/auth/sessions')
    return response.data
  }
}
