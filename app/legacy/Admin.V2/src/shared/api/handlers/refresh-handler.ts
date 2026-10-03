import axios from 'axios'
import type { Result } from '@/shared/models'
import { STORAGE_KEYS } from '@/shared/constants'

export async function refreshTokens(): Promise<boolean> {
  const token = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)
  if (!token) {
    return false
  }

  try {
    const refreshResponse = await axios.post('/api/storefront/identity/auth/sessions/refresh', {
      refreshToken: token,
    })

    const body = refreshResponse.data as Record<string, unknown>
    if (body && 'value' in body) {
      const value = (body as unknown as Result<Record<string, unknown>>).value
      const accessToken = value.accessToken as string
      const newRefreshToken = value.refreshToken as string

      localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken)
      localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken)

      return true
    }

    return false
  } catch {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
    return false
  }
}
