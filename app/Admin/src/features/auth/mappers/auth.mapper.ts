import type { LoginResponse } from '../../auth/types/login.response.type'
import type { AuthSession } from '../../auth/types/auth.model.type'

export function mapAuthSession(login: LoginResponse, session: { id: string; roles: string[]; permissions: string[] } | null): AuthSession {
  return {
    accessToken: login.accessToken,
    accessTokenExpiresIn: login.accessTokenExpiresIn,
    refreshToken: login.refreshToken,
    refreshTokenExpiresIn: login.refreshTokenExpiresIn,
    user: session ? { id: session.id, roles: session.roles, permissions: session.permissions } : null,
  }
}
