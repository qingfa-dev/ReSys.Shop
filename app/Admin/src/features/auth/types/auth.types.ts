import type { ApiResult } from '@/shared/api/types/api.types'
import type { LoginFormData } from '../schemas/auth.schema'

export type LoginRequest = LoginFormData & {
  ipAddress?: string
}

export interface RefreshRequest {
  refreshToken: string
  rememberMe?: boolean
  ipAddress?: string
}

export interface AuthenticationResponse {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface UserProfile {
  id: string
  email: string
  fullName: string
  roles: string[]
}

export type { ApiResult }
