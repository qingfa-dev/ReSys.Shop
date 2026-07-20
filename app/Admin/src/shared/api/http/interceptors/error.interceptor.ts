import type { AxiosError, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import type { ServerResult } from '../../../api/types/result.type'
import { parseApiError } from '../handlers/error.normalizer'
import { tokenService } from '../services/token.service'
import { refreshTokens } from '../handlers/refresh.handler'

function createErrorResult(statusCode: number, message: string | null, detail: string | null, errorCode: string | undefined): ServerResult<null> {
  return {
    isSuccess: false,
    statusCode,
    errors: [{ code: errorCode || 'ERROR', message: detail || message || 'Request failed', type: 0, metadata: null }],
    message,
    metadata: null,
    value: null,
  }
}

export async function errorInterceptor(error: AxiosError): Promise<AxiosResponse> {
  const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
  const apiError = parseApiError(error)

  if (apiError.statusCode === 401 && originalRequest && !originalRequest._retry) {
    if (originalRequest.url?.includes('/auth/session/refresh')) {
      return Promise.resolve({
        data: createErrorResult(401, apiError.title, apiError.detail || 'Unauthorized', 'UNAUTHORIZED'),
      } as AxiosResponse)
    }

    console.warn('Session expired. Attempting to refresh token...')

    originalRequest._retry = true
    const refreshed = await refreshTokens()
    if (refreshed) {
      const newToken = tokenService.getAccessToken()
      if (newToken && originalRequest.headers) {
        originalRequest.headers.Authorization = `Bearer ${newToken}`
      }
      const { default: apiClient } = await import('../api.client')
      return apiClient(originalRequest)
    }
  }

  return Promise.resolve({
    data: createErrorResult(apiError.statusCode, apiError.title, apiError.detail, apiError.errorCode),
  } as AxiosResponse)
}
