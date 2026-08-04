import type { ServerResult } from '@/common/api/types/result.types'
import type { LoginResponse } from '../../auth/types/login.response'
import type { AuthSession } from '../../auth/models/auth.model'

export function mapAuthSession(
  result: ServerResult<LoginResponse>,
  session: { id: string; roles: string[]; permissions: string[] } | null,
): ServerResult<AuthSession> {
  return {
    ...result,
    value: {
      accessToken: result.value.accessToken,
      accessTokenExpiresIn: result.value.accessTokenExpiresIn,
      refreshToken: result.value.refreshToken,
      refreshTokenExpiresIn: result.value.refreshTokenExpiresIn,
      user: session ? { id: session.id, roles: session.roles, permissions: session.permissions } : null,
    },
  }
}
