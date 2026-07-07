import axios, { type AxiosInstance, type AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import type { ServerResult, ServerPagedResult } from '../types/result.types'
import { parseApiError } from '../utils/api.utils'
import { refreshTokens } from './refresh-handler'

const apiClient: AxiosInstance = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
  paramsSerializer: {
    indexes: null,
  },
})

apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('accessToken')
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  },
)

apiClient.interceptors.response.use(
  (response) => {
    const body = response.data as Record<string, unknown>

    if (body && typeof body === 'object') {
      if ('items' in body && Array.isArray(body.items)) {
        const paged = body as unknown as ServerPagedResult<unknown>
        return {
          data: paged.items,
          meta: {
            page: paged.page,
            pageSize: paged.pageSize,
            totalCount: paged.totalCount,
            totalPages: Math.ceil(paged.totalCount / paged.pageSize),
          },
          success: true,
        } as unknown as AxiosResponse
      }

      if ('value' in body) {
        return {
          data: (body as unknown as ServerResult<unknown>).value,
          success: true,
        } as unknown as AxiosResponse
      }
    }

    return {
      data: body,
      success: true,
    } as unknown as AxiosResponse
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
    const apiError = parseApiError(error)

    if (apiError.statusCode === 401 && originalRequest && !originalRequest._retry) {
      console.warn('Session expired. Attempting to refresh token...')

      if (originalRequest.url?.includes('/auth/session/refresh')) {
        return Promise.resolve({ data: null, success: false, error: apiError })
      }

      originalRequest._retry = true

      const refreshed = await refreshTokens()
      if (refreshed) {
        const newToken = localStorage.getItem('accessToken')
        if (newToken && originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${newToken}`
        }
        return apiClient(originalRequest)
      }
    }

    return Promise.resolve({
      data: null,
      success: false,
      error: apiError,
    })
  },
)

export default apiClient
