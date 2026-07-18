import type { LoginParameters } from '../schemas/Login.Schema'

export type LoginRequest = LoginParameters & {
  ipAddress?: string
}

export interface RefreshRequest {
  refreshToken: string
  rememberMe?: boolean
  ipAddress?: string
}
