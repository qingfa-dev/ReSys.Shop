import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { AuthResponse } from '../../types/response'
import type { LoginRequest, RegisterRequest } from '../../types/request'
import type { IAuthRepository } from './auth.repository.interface'

export class AuthApiRepository extends BaseRepository implements IAuthRepository {
  async login(credentials: LoginRequest): Promise<Result<AuthResponse>> {
    return this.post<AuthResponse>('/api/store/identity/auth/login/password', credentials)
  }

  async register(info: RegisterRequest): Promise<Result<AuthResponse>> {
    return this.post<AuthResponse>('/api/store/identity/auth/register', info)
  }

  async logout(): Promise<Result<void>> {
    return this.post<void>('/api/store/identity/auth/logout')
  }

  async requestPasswordReset(email: string): Promise<Result<void>> {
    return this.post<void>('/api/store/identity/passwords/forgot', { email })
  }
}

export const authApiRepository = new AuthApiRepository()