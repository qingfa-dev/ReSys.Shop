import axios, { type AxiosInstance, type AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import type { ApiResponse, ApiResult } from './api.types'
import { parseApiError } from './api.utils'
import { useToast } from '../composables/toast.use'

/**
 * Configured Axios instance with ReSys-specific interceptors.
 * It handles automatic unwrapping of the server envelope and global error notification.
 */
const apiClient: AxiosInstance = axios.create({
  baseURL: '/api', // Proxied via Vite config to the backend
  headers: {
    'Content-Type': 'application/json',
  },
  paramsSerializer: {
    indexes: null, // by default: false (no brackets)
  },
})

// --- REQUEST INTERCEPTOR ---
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
  }
)

// --- RESPONSE INTERCEPTOR ---
apiClient.interceptors.response.use(
  (response) => {
    /**
     * SUCCESS HANDLER
     * Unwrap the server's ApiResponse envelope to simplify data access in the UI.
     */
    const apiResponse = response.data as ApiResponse<unknown>

    const result: ApiResult<unknown> = {
      data: apiResponse.data,
      meta: apiResponse.meta,
      success: true,
    }

    return result as unknown as AxiosResponse
  },
  async (error: AxiosError) => {
    const { showToast } = useToast()
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
    const apiError = parseApiError(error)

    // 1. Handle 401 Unauthorized (Token Expiry)
    if (apiError.status === 401 && originalRequest && !originalRequest._retry) {
      console.warn('Session expired. Attempting to refresh token...');

      // Skip if this is already a refresh attempt to avoid loops
      if (originalRequest.url?.includes('/auth/session/refresh')) {
        return Promise.resolve({ data: null, success: false, error: apiError })
      }

      originalRequest._retry = true
      const refreshToken = localStorage.getItem('refreshToken')

      if (refreshToken) {
        try {
          // Perform refresh using a clean axios call to bypass interceptors
          // Note: We assume the refresh endpoint structure matches the backend
          const refreshResponse = await axios.post('/api/auth/session/refresh', {
            refreshToken: refreshToken
          });

          if (refreshResponse.data && refreshResponse.data.data) {
            const { accessToken, refreshToken: newRefreshToken } = refreshResponse.data.data;

            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', newRefreshToken);

            // Update header for the retry
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${accessToken}`;
            }

            // Retry original request
            return apiClient(originalRequest);
          }
        } catch (refreshErr) {
          console.error('Session refresh failed', refreshErr);
          // Clear tokens on failure
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');

          // Redirect to login is handled by the router guard or component
          // when they detect the 401/unauthenticated state.
          // We could also force a reload or event here.
          window.location.href = '/login';
        }
      }
    }

    // Global Toast for non-401 errors
    if (apiError.status !== 401) {
      let detail = apiError.detail || 'An unexpected error occurred.'

      // If we have validation errors, append them
      if (apiError.errors) {
        const firstError = Object.values(apiError.errors)[0]
        if (firstError && Array.isArray(firstError) && firstError.length > 0) {
          detail = firstError[0] || detail
        }
      }

      showToast('error', apiError.title || 'Error', detail)
    }

    // 2. Standardized Result Pattern return
    // We swallow the rejection and return a structured failure object.
    return Promise.resolve({
      data: null,
      success: false,
      error: apiError,
    })
  },
)

export default apiClient
