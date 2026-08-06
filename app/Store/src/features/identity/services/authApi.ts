import { post, get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'
import type { TokenPair, LoginRequest, RegisterRequest, SessionUser } from '../types/auth'

export async function login(req: LoginRequest): Promise<Result<TokenPair>> {
  return post<Result<TokenPair>>(ENDPOINTS.authLoginPassword, req)
}

export async function register(req: RegisterRequest): Promise<Result<unknown>> {
  return post<Result<unknown>>(ENDPOINTS.authRegister, req)
}

export async function logout(req?: { revokeAll?: boolean }): Promise<void> {
  await post(ENDPOINTS.authLogout, req)
}

export async function getSession(): Promise<Result<SessionUser>> {
  return get<Result<SessionUser>>(ENDPOINTS.sessions)
}

export async function getLoginProviders(): Promise<Result<Array<{ name: string; url: string }>>> {
  return get<Result<Array<{ name: string; url: string }>>>(ENDPOINTS.authLoginProviders)
}

export async function forgotPassword(email: string): Promise<Result<unknown>> {
  return post<Result<unknown>>(ENDPOINTS.passwordsForgot, { email })
}

export async function resetPassword(token: string, newPassword: string): Promise<Result<unknown>> {
  return post<Result<unknown>>(ENDPOINTS.passwordsReset, { token, newPassword })
}

export async function changePassword(
  currentPassword: string,
  newPassword: string,
): Promise<Result<void>> {
  return post('api/store/identity/passwords/change', { currentPassword, newPassword })
}
