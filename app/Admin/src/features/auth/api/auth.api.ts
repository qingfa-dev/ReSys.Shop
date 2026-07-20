import { apiClient } from '@/common/api'
import type { ServerResult } from '@/shared/types/result.type'
import type { LoginResponse } from '../types/login.response.type'
import type { LoginRequest } from '../types/login.request.type'
import type { RefreshTokenRequest, AuthProfileResponse } from '../types/auth.request.type'
import type { AuthSession } from '../types/auth.model.type'

const BASE_URL = '/store/identity/auth'

function path(sub: string): string {
  return `${BASE_URL}/${sub}`
}

function mapAuthSession(login: LoginResponse, session: { id: string; roles: string[]; permissions: string[] } | null): AuthSession {
  return {
    accessToken: login.accessToken,
    accessTokenExpiresIn: login.accessTokenExpiresIn,
    refreshToken: login.refreshToken,
    refreshTokenExpiresIn: login.refreshTokenExpiresIn,
    user: session ? { id: session.id, roles: session.roles, permissions: session.permissions } : null,
  }
}

async function fetchSession(): Promise<{ id: string; roles: string[]; permissions: string[] } | null> {
  try {
    const res = await apiClient.get(path('profile'))
    const data = res.data as ServerResult<AuthProfileResponse>
    if (data.isSuccess && data.value) {
      return {
        id: data.value.id,
        roles: Array.isArray(data.value.roles) ? data.value.roles : [],
        permissions: Array.isArray(data.value.permissions) ? data.value.permissions : [],
      }
    }
  } catch {
    /* ignore */
  }
  return null
}

export const authService = {
  async login(request: LoginRequest): Promise<ServerResult<AuthSession>> {
    const res = await apiClient.post(path('login/password'), request)
    const result = res.data as ServerResult<LoginResponse>
    if (!result.isSuccess) return result as unknown as ServerResult<AuthSession>
    const session = await fetchSession()
    return {
      ...result,
      value: mapAuthSession(result.value, session),
    } as ServerResult<AuthSession>
  },

  async refresh(request: RefreshTokenRequest): Promise<ServerResult<AuthSession>> {
    const res = await apiClient.post(path('sessions/refresh'), request)
    const result = res.data as ServerResult<LoginResponse>
    if (!result.isSuccess) return result as unknown as ServerResult<AuthSession>
    const session = await fetchSession()
    return {
      ...result,
      value: mapAuthSession(result.value, session),
    } as ServerResult<AuthSession>
  },

  async logout(): Promise<ServerResult<void>> {
    const res = await apiClient.post(path('logout'), {})
    return res.data as ServerResult<void>
  },

  async getProfile(): Promise<ServerResult<unknown>> {
    const res = await apiClient.get(path('profile'))
    return res.data as ServerResult<unknown>
  },
}
