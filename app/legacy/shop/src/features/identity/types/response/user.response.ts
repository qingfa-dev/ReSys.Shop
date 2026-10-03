import type { Result } from '@/core/models/result'

export interface UserResponse {
  id: string
  email: string
  firstName: string
  lastName: string
  phone?: string
  avatar?: string
  role: string
  emailVerified: boolean
  mfaEnabled: boolean
  createdAt: string
  updatedAt: string
}

export interface AuthTokensResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
}

export interface AuthResponse {
  user: UserResponse
  tokens: AuthTokensResponse
}

export type UserSingleResponse = Result<UserResponse>
export type AuthSingleResponse = Result<AuthResponse>
export type UserListResponse = import('@/core/models/result').PagedResult<UserResponse>