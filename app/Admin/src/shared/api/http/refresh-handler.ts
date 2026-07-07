import axios from 'axios'
import type { ServerResult } from '../types/result.types'

export async function refreshTokens(): Promise<boolean> {
  const token = localStorage.getItem('refreshToken')
  if (!token) {
    window.location.href = '/login'
    return false
  }

  try {
    const refreshResponse = await axios.post('/api/auth/session/refresh', {
      refreshToken: token,
    })

    const body = refreshResponse.data as Record<string, unknown>
    if (body && 'value' in body) {
      const { accessToken, refreshToken: newRefreshToken } = (body as unknown as ServerResult<{ accessToken: string; refreshToken: string }>).value

      localStorage.setItem('accessToken', accessToken)
      localStorage.setItem('refreshToken', newRefreshToken)

      return true
    }

    return false
  } catch {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    window.location.href = '/login'
    return false
  }
}
