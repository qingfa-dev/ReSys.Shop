import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import type { Result } from '@/core/models/result'

const TOKEN_KEY = 'accessToken'
const REFRESH_TOKEN_KEY = 'refreshToken'

interface RefreshResponse {
  accessToken: string
  refreshToken: string
}

export function responseInterceptor<T>(response: { data: T }): T {
  return response.data
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
        const baseURL = originalRequest.baseURL || '/api'
        const { data } = await axios.post<RefreshResponse>(`${baseURL}/identity/auth/refresh`, {
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
