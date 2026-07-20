import type { LoginParameters } from '../schemas/login.schema'

export type LoginRequest = LoginParameters & {
  ipAddress?: string
}
