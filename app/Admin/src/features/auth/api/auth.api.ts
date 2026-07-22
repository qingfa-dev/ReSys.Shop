import apiClient from '@/shared/api/client'
import { AuthService } from '@/shared/auth/auth.service'
import type { Result } from '@/shared/models'
import type {
  RegisterRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
  TokenResponse,
  RegisterResponse,
  SessionResponse,
} from '../types'

export async function loginApi(credential: string, password: string): Promise<Result<TokenResponse>> {
  return AuthService.login({ email: credential, password }) as Promise<Result<TokenResponse>>
}

export async function registerApi(fields: RegisterRequest): Promise<Result<RegisterResponse>> {
  const response = await apiClient.post<Result<RegisterResponse>>('/store/identity/auth/register', fields)
  return response.data
}

export async function forgotPasswordApi(email: string): Promise<Result<null>> {
  const response = await apiClient.post<Result<null>>('/store/identity/passwords/forgot', { email })
  return response.data
}

export async function resetPasswordApi(params: ResetPasswordRequest): Promise<Result<null>> {
  const response = await apiClient.post<Result<null>>('/store/identity/passwords/reset', params)
  return response.data
}

export async function changePasswordApi(params: ChangePasswordRequest): Promise<Result<null>> {
  const response = await apiClient.post<Result<null>>('/store/identity/passwords/change', params)
  return response.data
}

export async function logoutApi(): Promise<void> {
  await AuthService.logout()
}

export async function getSessionApi(): Promise<Result<SessionResponse>> {
  return AuthService.getCurrentUser() as Promise<Result<SessionResponse>>
}
