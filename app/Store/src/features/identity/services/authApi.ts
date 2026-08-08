import { get, post } from '@/shared/api/client'
import { IDENTITY } from '@/shared/constants/api'
import { TokenPairSchema, SessionUserSchema } from '../validations/auth'
import type { Result } from '@/shared/types'
import type { LoginRequest, RegisterRequest, TokenPair, SessionUser } from '../types'

export class AuthApi {
  static async login(req: LoginRequest): Promise<Result<TokenPair>> {
    const result = await post<Result<TokenPair>>(`${IDENTITY}/auth/login/password`, req)
    if (!result.isSuccess) return result
    result.value = TokenPairSchema.parse(result.value)
    return result
  }

  static async register(req: RegisterRequest): Promise<Result<void>> {
    return await post<Result<void>>(`${IDENTITY}/auth/register`, req)
  }

  static async logout(req?: { refreshToken?: string; revokeAll?: boolean }): Promise<Result<void>> {
    return await post<Result<void>>(`${IDENTITY}/auth/logout`, req ?? {})
  }

  static async getSession(): Promise<Result<SessionUser>> {
    const result = await get<Result<SessionUser>>(`${IDENTITY}/auth/sessions`)
    if (!result.isSuccess) return result
    result.value = SessionUserSchema.parse(result.value)
    return result
  }

  static async getLoginProviders(): Promise<Result<{ name: string; url: string }[]>> {
    return await get<Result<{ name: string; url: string }[]>>(`${IDENTITY}/auth/login/providers`)
  }

  static async forgotPassword(email: string): Promise<Result<void>> {
    return await post<Result<void>>(`${IDENTITY}/passwords/forgot`, { email })
  }

  static async resetPassword(token: string, newPassword: string): Promise<Result<void>> {
    return await post<Result<void>>(`${IDENTITY}/passwords/reset`, { token, newPassword })
  }

  static async changePassword(currentPassword: string, newPassword: string): Promise<Result<void>> {
    return await post<Result<void>>(`${IDENTITY}/passwords/change`, { currentPassword, newPassword })
  }
}
