import type { AuthSessionResponse } from '../types/login.response'

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

export function toAuthSessionModel(dto: { login: { accessToken: string; accessTokenExpiresIn: number; refreshToken: string; refreshTokenExpiresIn: number }; session: AuthSessionResponse | null }): AuthSession {
  return {
    accessToken: dto.login.accessToken,
    accessTokenExpiresIn: dto.login.accessTokenExpiresIn,
    refreshToken: dto.login.refreshToken,
    refreshTokenExpiresIn: dto.login.refreshTokenExpiresIn,
    user: dto.session ? { id: dto.session.id, roles: dto.session.roles, permissions: dto.session.permissions } : null,
  }
}
