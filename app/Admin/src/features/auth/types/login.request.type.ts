import type { LoginParameters } from '../schemas/login.schema'

export type LoginRequest = LoginParameters & {
  ipAddress?: string
}

export interface RefreshRequest {
  refreshToken: string
  rememberMe?: boolean
  ipAddress?: string
}
