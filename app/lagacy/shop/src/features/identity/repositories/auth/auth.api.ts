import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { AuthResponse } from '../../types/response'
import type { LoginRequest, RegisterRequest } from '../../types/request'
import type { IAuthRepository } from './auth.repository.interface'

export class AuthApiRepository extends BaseRepository implements IAuthRepository {
  async login(credentials: LoginRequest): Promise<Result<AuthResponse>> {
    return this.post<AuthResponse>('/identity/auth/login', credentials)
  }

  async register(info: RegisterRequest): Promise<Result<AuthResponse>> {
    return this.post<AuthResponse>('/identity/auth/register', info)
  }

  async logout(): Promise<Result<void>> {
    return this.post<void>('/identity/auth/logout')
  }

  async requestPasswordReset(email: string): Promise<Result<void>> {
    return this.post<void>('/identity/auth/forgot-password', { email })
  }

  async enableMFA(userId: string): Promise<Result<{ secret: string; qrCode: string }>> {
    return this.post<{ secret: string; qrCode: string }>('/identity/auth/mfa/enable', { userId })
  }

  async verifyMFA(userId: string, code: string): Promise<Result<void>> {
    return this.post<void>('/identity/auth/mfa/verify', { userId, code })
  }

  async disableMFA(userId: string, code: string): Promise<Result<void>> {
    return this.post<void>('/identity/auth/mfa/disable', { userId, code })
  }
}

export const authApiRepository = new AuthApiRepository()