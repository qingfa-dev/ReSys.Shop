import apiClient from '@/shared/api/http/api.client'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { LoginRequest } from '../types/auth.request.types'

interface AuthDto {
  access_token: string
  access_token_expires_in: number
  refresh_token: string
  refresh_token_expires_in: number
}

const BASE_URL = '/store/identity/auth'

function path(sub: string): string {
  return `${BASE_URL}/${sub}`
}

export const authRepository = {
  async login(request: LoginRequest): Promise<ServerResult<AuthDto>> {
    const res = await apiClient.post(path('login/password'), request)
    return res.data as ServerResult<AuthDto>
  },

  async refresh(request: { refreshToken: string; rememberMe?: boolean }): Promise<ServerResult<AuthDto>> {
    const res = await apiClient.post(path('sessions/refresh'), request)
    return res.data as ServerResult<AuthDto>
  },

  async logout(): Promise<ServerResult<void>> {
    const res = await apiClient.post(path('logout'), {})
    return res.data as ServerResult<void>
  },

  async getProfile(): Promise<ServerResult<Record<string, unknown>>> {
    const res = await apiClient.get(path('profile'))
    return res.data as ServerResult<Record<string, unknown>>
  },

  async updateProfile(data: Record<string, unknown>): Promise<ServerResult<void>> {
    const res = await apiClient.put(path('profile'), data)
    return res.data as ServerResult<void>
  },

  async changePassword(data: { current_password: string; new_password: string; confirm_new_password: string }): Promise<ServerResult<void>> {
    const res = await apiClient.post(path('password/change'), data)
    return res.data as ServerResult<void>
  },
}
