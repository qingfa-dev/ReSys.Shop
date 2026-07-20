import type { LoginParameters } from '../types/login.field'

export type LoginRequest = LoginParameters & {
  ipAddress?: string
}

export interface RefreshRequest {
  refreshToken: string
  rememberMe?: boolean
  ipAddress?: string
}
