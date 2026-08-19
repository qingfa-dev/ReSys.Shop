import axios from 'axios'
import { STORAGE_KEYS } from '@/shared/constants/storage'

// Assign: Refresh endpoint URL — configurable for different environments
let refreshUrl: string = '/api/storefront/identity/auth/sessions/refresh'
// Acquire: Raw axios instance bypasses interceptors to avoid infinite refresh loop
const rawAxios = axios.create()

// State: Single-flight refresh — concurrent callers queue behind the first
let isRefreshing = false
let pendingQueue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = []

// Assign: Override refresh URL for environment-specific endpoints
export function setRefreshUrl(url: string) {
  refreshUrl = url
}

// Retry: Single-flight token refresh — queues concurrent requests until first completes
export async function handleTokenRefresh(): Promise<string> {
  if (isRefreshing) {
    return new Promise<string>((resolve, reject) => {
      pendingQueue.push({ resolve, reject })
    })
  }

  isRefreshing = true

  try {
    // Check: Refresh token must exist before attempting refresh
    const token = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)
    if (!token) {
      throw new Error('No refresh token available')
    }

    // Call: Auth provider refresh endpoint — returns new access + optional refresh token
    const response = await rawAxios.post(refreshUrl, { refreshToken: token })
    const data = (response.data as Record<string, unknown>)?.value as Record<string, unknown> | undefined
    const body = data ?? (response.data as Record<string, unknown>)
    const accessToken = body?.accessToken as string | undefined
    const newRefreshToken = body?.refreshToken as string | undefined

    // Validate: Response must contain an access token
    if (!accessToken) {
      throw new Error('Invalid refresh response — no access token')
    }

    // Update: Persist new tokens — refresh token rotates if provided
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken)
    if (newRefreshToken) {
      localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken)
    }

    // Notify: Resolve all queued callers with the new access token
    pendingQueue.forEach(p => p.resolve(accessToken))
    return accessToken
  } catch (e) {
    // Purge: Clear all tokens on refresh failure — forces re-login
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
    pendingQueue.forEach(p => p.reject(e))
    throw e
  } finally {
    pendingQueue = []
    isRefreshing = false
  }
}
