import axios, { type AxiosInstance, type AxiosError, type InternalAxiosRequestConfig } from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'
const TOKEN_KEY = 'accessToken'
const REFRESH_TOKEN_KEY = 'refreshToken'

export const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000,
})

api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem(TOKEN_KEY)
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
    const cartToken = localStorage.getItem('cartToken')
    if (cartToken && config.headers) {
      config.headers['X-Cart-Token'] = cartToken
    }
    return config
  },
  (error) => Promise.reject(error)
)

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
        if (refreshToken) {
          const { data } = await axios.post(`${API_BASE_URL}/api/store/identity/auth/sessions/refresh`, {
            refreshToken,
          })

          localStorage.setItem(TOKEN_KEY, data.accessToken)
          localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken)

          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${data.accessToken}`
          }
          return api(originalRequest)
        }
      } catch (refreshError) {
        localStorage.removeItem(TOKEN_KEY)
        localStorage.removeItem(REFRESH_TOKEN_KEY)
        window.location.href = '/login'
        return Promise.reject(refreshError)
      }
    }

    return Promise.reject(error)
  }
)

export function setAuthToken(token: string, refreshToken?: string): void {
  localStorage.setItem(TOKEN_KEY, token)
  if (refreshToken) {
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
  }
}

export function clearAuthToken(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}

export function getAuthToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export default api
