import axios from 'axios'
import type { ServerResult } from '../../../api/types/result.type'
import { tokenService } from '../services/token.service'

export async function refreshTokens(): Promise<boolean> {
  const token = tokenService.getRefreshToken()
  if (!token) {
    window.location.href = '/login'
    return false
  }

  try {
    const refreshResponse = await axios.post('/api/store/identity/auth/sessions/refresh', {
      refreshToken: token,
    })

    const body = refreshResponse.data as Record<string, unknown>
    if (body && 'value' in body) {
      const value = (body as unknown as ServerResult<Record<string, unknown>>).value
      const accessToken = value.accessToken as string
      const newRefreshToken = value.refreshToken as string

      tokenService.setTokens(accessToken, newRefreshToken)
      return true
    }

    return false
  } catch {
    tokenService.clearTokens()
    window.location.href = '/login'
    return false
  }
}
