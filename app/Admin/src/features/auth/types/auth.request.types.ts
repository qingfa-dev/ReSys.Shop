import type { LoginFormData } from './auth.model.types'

export type LoginRequest = LoginFormData & {
  ipAddress?: string
}

export interface RefreshRequest {
  refreshToken: string
  rememberMe?: boolean
  ipAddress?: string
}
