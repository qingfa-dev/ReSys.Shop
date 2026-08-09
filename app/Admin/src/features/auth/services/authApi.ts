import { post, get } from '@/shared/api/client'
import type { Result } from '@/shared/types/result'
import type {
  LoginRequest,
  LogoutRequest,
  ResetPasswordRequest,
  ForgotPasswordRequest,
  TokenPair,
  SessionInfo,
} from '../types/auth'

const AUTH_BASE = 'api/storefront/identity/auth'
const PASSWORD_BASE = 'api/storefront/identity/passwords'

export function login(request: LoginRequest): Promise<Result<TokenPair>> {
  return post<Result<TokenPair>>(`${AUTH_BASE}/login/password`, request)
}

export function logout(request?: LogoutRequest): Promise<Result<void>> {
  return post<Result<void>>(`${AUTH_BASE}/logout`, request ?? undefined)
}

export function getSession(): Promise<Result<SessionInfo>> {
  return get<Result<SessionInfo>>(`${AUTH_BASE}/sessions`)
}

export function forgotPassword(request: ForgotPasswordRequest): Promise<void> {
  return post<void>(`${PASSWORD_BASE}/forgot`, request)
}

export function resetPassword(request: ResetPasswordRequest): Promise<Result<void>> {
  return post<Result<void>>(`${PASSWORD_BASE}/reset`, request)
}
