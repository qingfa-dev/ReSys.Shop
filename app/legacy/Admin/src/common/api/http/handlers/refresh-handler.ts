import axios from 'axios'
import type { ServerResult } from '../../types/result.types'

export async function refreshTokens(): Promise<boolean> {
  const token = localStorage.getItem('refreshToken')
  if (!token) {
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

      localStorage.setItem('accessToken', accessToken)
      localStorage.setItem('refreshToken', newRefreshToken)

      return true
    }

    return false
  } catch {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    return false
  }
}
