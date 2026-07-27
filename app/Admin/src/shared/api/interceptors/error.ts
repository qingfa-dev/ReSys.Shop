import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'
import type { ApiError } from '@/shared/types/error'
import { HttpError } from '../errors'
import { handleTokenRefresh } from './refresh'
import { getApiClient } from '../axios'

function extractErrors(
  data: Record<string, unknown> | undefined,
  status: number,
): ApiError[] {
  if (data?.errors && Array.isArray(data.errors)) {
    return (data.errors as Array<{ code: string; message: string; type?: number }>).map(e => ({
      code: e.code,
      message: e.message,
      type: e.type ?? status,
    }))
  }

  if (typeof data?.title === 'string') {
    return [{ code: (data.code as string) ?? 'HttpError', message: data.title as string, type: status }]
  }

  return [{ code: 'HttpError', message: `HTTP ${status}`, type: status }]
}

export async function errorInterceptor(error: unknown): Promise<never> {
  if (axios.isCancel(error)) {
    return Promise.reject(error)
  }

  if (!axios.isAxiosError(error)) {
    return Promise.reject(new HttpError(0, [{ code: 'Unexpected', message: 'An unexpected error occurred.', type: 0 }]))
  }

  const status = error.response?.status ?? 0
  const data = error.response?.data as Record<string, unknown> | undefined

  if (status === 401) {
    const config = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
    if (config && !config._retry && !config.url?.includes('/sessions/refresh')) {
      config._retry = true
      try {
        const newToken = await handleTokenRefresh()
        config.headers.Authorization = `Bearer ${newToken}`
        const response = await getApiClient().request(config)
        return response as never
      } catch {
        return Promise.reject(new HttpError(401, [{ code: 'Unauthorized', message: 'Session expired. Please log in again.', type: 401 }]))
      }
    }
  }

  const errors = extractErrors(data, status)
  return Promise.reject(new HttpError(status, errors))
}
