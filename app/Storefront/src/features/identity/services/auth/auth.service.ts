import { authApiRepository } from '../../repositories/auth/auth.api'
import { mockAuthRepository } from '../../repositories/auth/auth.mock.repository'
import type { IAuthService, AuthResponse } from './auth.service.interface'
import type { LoginRequest, RegisterRequest } from '../../types'
import type { Result } from '@/core/models/result'
import { mapAuthResponseToEntity } from '../../mapping'
import { resultMap, succeed, fail } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class AuthService implements IAuthService {
  private readonly authRepository = USE_MOCK ? mockAuthRepository : authApiRepository

  async login(credentials: LoginRequest): Promise<Result<AuthResponse>> {
    const response = await this.authRepository.login(credentials)
    return resultMap(response, mapAuthResponseToEntity)
  }

  async register(info: RegisterRequest): Promise<Result<AuthResponse>> {
    const response = await this.authRepository.register(info)
    return resultMap(response, mapAuthResponseToEntity)
  }

  async logout(): Promise<Result<void>> {
    const response = await this.authRepository.logout() as Result<void>
    if (response.isFailure) {
      return fail(response.message ?? 'Logout failed', response.statusCode, response.errors)
    }
    return succeed(undefined, response.statusCode)
  }

  async requestPasswordReset(email: string): Promise<Result<void>> {
    const response = await this.authRepository.requestPasswordReset(email) as Result<void>
    if (response.isFailure) {
      return fail(response.message ?? 'Password reset request failed', response.statusCode, response.errors)
    }
    return succeed(undefined, response.statusCode)
  }
}

export const authService = new AuthService()