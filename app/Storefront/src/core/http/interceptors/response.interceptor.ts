import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import type { Result } from '@/core/models/result'

const TOKEN_KEY = 'accessToken'
const REFRESH_TOKEN_KEY = 'refreshToken'

interface RefreshResponse {
  accessToken: string
  refreshToken: string
}

export function responseInterceptor<T>(response: T): T {
  // Return response as-is — BaseRepository handles unwrapping via response.data.
  return response
}

export async function responseErrorInterceptor(
  error: AxiosError<Result<unknown>>
): Promise<never> {
  const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

  if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
    originalRequest._retry = true

    try {
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
      if (refreshToken) {
        const { data } = await axios.post<RefreshResponse>(`/api/store/identity/auth/sessions/refresh`, {
          refreshToken,
        })

        localStorage.setItem(TOKEN_KEY, data.accessToken)
        localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken)

        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${data.accessToken}`
        }
        
        return axios(originalRequest)
      }
    } catch {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(REFRESH_TOKEN_KEY)
      window.location.href = '/login'
    }
  }

  return Promise.reject(error)
}
