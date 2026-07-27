import { type AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import type { Result } from '@/shared/models'
import { parseApiError } from '../handlers/error-handler'
import { refreshTokens } from '../handlers/refresh-handler'
import apiClient from '../client'
import router from '@/router'
import { STORAGE_KEYS } from '@/shared/constants'

export async function errorWrapperInterceptor(error: AxiosError): Promise<AxiosResponse> {
  const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
  const apiError = parseApiError(error)

  if (apiError.statusCode === 401 && originalRequest && !originalRequest._retry) {
    if (originalRequest.url?.endsWith('/sessions/refresh')) {
      return Promise.resolve({
        data: {
          isSuccess: false,
          statusCode: 401,
          errors: [
            {
              code: 'UNAUTHORIZED',
              message: apiError.detail || 'Unauthorized',
              type: 0,
              metadata: null,
            },
          ],
          message: apiError.title,
          metadata: null,
          value: null,
        } as Result<null>,
      } as AxiosResponse)
    }

    console.warn('Session expired. Attempting to refresh token...')

    originalRequest._retry = true
    const refreshed = await refreshTokens()
    if (refreshed) {
      const newToken = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
      if (newToken && originalRequest.headers) {
        originalRequest.headers.Authorization = `Bearer ${newToken}`
      }
      return apiClient(originalRequest)
    } else {
      router.push('/login')
      return Promise.reject(error)
    }
  }

  return Promise.resolve({
    data: {
      isSuccess: false,
      statusCode: apiError.statusCode,
      errors: [
        {
          code: apiError.errorCode || 'ERROR',
          message: apiError.detail || apiError.title || 'Request failed',
          type: 0,
          metadata: null,
        },
      ],
      message: apiError.title,
      metadata: null,
      value: null,
    } as Result<null>,
  } as AxiosResponse)
}
