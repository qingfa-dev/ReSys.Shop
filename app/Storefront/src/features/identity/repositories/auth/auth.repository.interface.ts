import type { Result } from '@/core/models/result'
import type { AuthResponse } from '../../types/response'
import type { LoginRequest, RegisterRequest } from '../../types/request'

export interface IAuthRepository {
  login(credentials: LoginRequest): Promise<Result<AuthResponse>>
  register(info: RegisterRequest): Promise<Result<AuthResponse>>
  logout(): Promise<Result<void>>
  refresh(refreshToken: string): Promise<Result<AuthResponse>>
  requestPasswordReset(email: string): Promise<Result<void>>
}