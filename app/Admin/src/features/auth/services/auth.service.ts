import { authRepository } from '../api/auth.api'
import type { ServerResult } from '@/shared/api/types/result.type'
import type { LoginRequest } from '../types/login.request.type'
import type { AuthSession } from '../types/auth.model.type'

export const authService = {
  async login(request: LoginRequest): Promise<ServerResult<AuthSession>> {
    return authRepository.login(request)
  },

  async refresh(request: { refreshToken: string; rememberMe?: boolean }): Promise<ServerResult<AuthSession>> {
    return authRepository.refresh(request)
  },

  async logout(): Promise<ServerResult<void>> {
    return authRepository.logout()
  },
}
