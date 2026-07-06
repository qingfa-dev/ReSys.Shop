import type { IsoDateString } from '@/shared/types/timestamp'
import type { UserId } from '@/shared/types/id'

export interface LoginRequest {
  email: string
  password: string
}

export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresAt: IsoDateString
}

export interface AuthUser {
  id: UserId
  email: string
  displayName: string
  roles: string[]
  permissions: string[]
}
