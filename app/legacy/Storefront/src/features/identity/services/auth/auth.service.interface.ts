import type { Result } from '@/core/models/result'
import type { LoginRequest, RegisterRequest, AuthTokens, User } from '../../types'

export interface IAuthService {
  login(credentials: LoginRequest): Promise<Result<AuthResponse>>
  register(info: RegisterRequest): Promise<Result<AuthResponse>>
  logout(): Promise<Result<void>>
  requestPasswordReset(email: string): Promise<Result<void>>
}

export interface AuthResponse {
  user: User
  tokens: AuthTokens
}