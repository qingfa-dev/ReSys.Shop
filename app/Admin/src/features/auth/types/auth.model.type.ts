import type { LoginResponse } from './login.response.type'

export interface AuthSession {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
  user: {
    id: string
    roles: string[]
    permissions: string[]
  } | null
}
