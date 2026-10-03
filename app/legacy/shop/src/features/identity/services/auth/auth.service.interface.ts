import type { Result } from '@/core/models/result'
import type { LoginRequest, RegisterRequest, AuthTokens, User } from '../../types'

export interface IAuthService {
  login(credentials: LoginRequest): Promise<Result<AuthResponse>>
  register(info: RegisterRequest): Promise<Result<AuthResponse>>
  logout(): Promise<Result<void>>
  requestPasswordReset(email: string): Promise<Result<void>>
  enableMFA(userId: string): Promise<Result<{ secret: string; qrCode: string }>>
  verifyMFA(userId: string, code: string): Promise<Result<void>>
  disableMFA(userId: string, code: string): Promise<Result<void>>
}

export interface AuthResponse {
  user: User
  tokens: AuthTokens
}