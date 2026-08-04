import axios from 'axios'
import { STORAGE_KEYS } from '@/shared/constants/storage'

let refreshUrl = '/api/store/identity/auth/sessions/refresh'
const rawAxios = axios.create()

let isRefreshing = false
let pendingQueue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = []

export function setRefreshUrl(url: string) {
  refreshUrl = url
}

export async function handleTokenRefresh(): Promise<string> {
  if (isRefreshing) {
    return new Promise<string>((resolve, reject) => {
      pendingQueue.push({ resolve, reject })
    })
  }

  isRefreshing = true

  try {
    const token = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)
    if (!token) {
      throw new Error('No refresh token available')
    }

    const response = await rawAxios.post(refreshUrl, { refreshToken: token })
    const data = (response.data as Record<string, unknown>)?.value as Record<string, unknown> | undefined
    const body = data ?? (response.data as Record<string, unknown>)
    const accessToken = body?.accessToken as string | undefined
    const newRefreshToken = body?.refreshToken as string | undefined

    if (!accessToken) {
      throw new Error('Invalid refresh response — no access token')
    }

    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken)
    if (newRefreshToken) {
      localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken)
    }

    pendingQueue.forEach(p => p.resolve(accessToken))
    return accessToken
  } catch (e) {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
    pendingQueue.forEach(p => p.reject(e))
    throw e
  } finally {
    pendingQueue = []
    isRefreshing = false
  }
}
