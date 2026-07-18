import apiClient from '@/shared/api/http/api.client'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { AuthenticationResponse } from '../types/Login.Response.Type'
import type { LoginRequest } from '../types/Login.Request.Type'
import type { ChangePasswordRequest } from '../types/ChangePassword.Request.Type'
import type { RefreshTokenRequest, UpdateProfileRequest, AuthProfileResponse } from '../types/Auth.Request.Type'

const BASE_URL = '/store/identity/auth'

function path(sub: string): string {
  return `${BASE_URL}/${sub}`
}

export const authRepository = {
  async login(request: LoginRequest): Promise<ServerResult<AuthenticationResponse>> {
    const res = await apiClient.post(path('login/password'), request)
    return res.data as ServerResult<AuthenticationResponse>
  },

  async refresh(request: RefreshTokenRequest): Promise<ServerResult<AuthenticationResponse>> {
    const res = await apiClient.post(path('sessions/refresh'), request)
    return res.data as ServerResult<AuthenticationResponse>
  },

  async logout(): Promise<ServerResult<void>> {
    const res = await apiClient.post(path('logout'), {})
    return res.data as ServerResult<void>
  },

  async getProfile(): Promise<ServerResult<AuthProfileResponse>> {
    const res = await apiClient.get(path('profile'))
    return res.data as ServerResult<AuthProfileResponse>
  },

  async updateProfile(data: UpdateProfileRequest): Promise<ServerResult<void>> {
    const res = await apiClient.put(path('profile'), data)
    return res.data as ServerResult<void>
  },

  async changePassword(data: ChangePasswordRequest): Promise<ServerResult<void>> {
    const res = await apiClient.post(path('password/change'), {
      currentPassword: data.currentPassword,
      newPassword: data.newPassword,
      confirmNewPassword: data.confirmNewPassword,
    })
    return res.data as ServerResult<void>
  },
}
