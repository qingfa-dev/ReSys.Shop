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
  access_token: string
  access_token_expires_at: number | string
  refresh_token: string
  refresh_token_expires_at: number | string
  token_type?: string
}

export interface UserProfile {
  id: string
  email: string
  fullName: string
  roles: string[]
}

export type { ApiResult }
