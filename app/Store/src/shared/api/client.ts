// Boundary: Shared → API layer — all HTTP calls funnel through this module
import { getApiClient } from './axios'
import { STORAGE_KEYS } from '@/shared/constants/storage'
export { HttpError } from './errors'

// Assign: Base URL for all outgoing API requests
export function setBaseUrl(url: string): void {
  getApiClient().defaults.baseURL = url
}

// Assign: Persist access token to localStorage — remove if null
export function setAuthToken(token: string | null): void {
  if (token) {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, token)
  } else {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
  }
}

export interface RequestConfig {
  signal?: AbortSignal
  headers?: Record<string, string>
}

// Call: GET request — returns deserialized response body
export async function get<T>(url: string, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().get<T>(url, { signal: config?.signal, headers: config?.headers })
  return response.data
}

// Transform: Merge caller headers with FormData auto-detection (omit Content-Type for multipart)
function requestConfig(body?: unknown, config?: RequestConfig): { signal?: AbortSignal; headers?: Record<string, string | undefined> } {
  const headers: Record<string, string | undefined> = { ...config?.headers }
  if (body instanceof FormData) {
    headers['Content-Type'] = undefined
  }
  return { signal: config?.signal, headers }
}

// Call: POST request — body serialized as JSON unless FormData
export async function post<T>(url: string, body?: unknown, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().post<T>(url, body, requestConfig(body, config))
  return response.data
}

// Call: PUT request — full resource replacement
export async function put<T>(url: string, body?: unknown, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().put<T>(url, body, requestConfig(body, config))
  return response.data
}

// Call: PATCH request — partial resource update
export async function patch<T>(url: string, body?: unknown, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().patch<T>(url, body, requestConfig(body, config))
  return response.data
}

// Call: DELETE request without body
export async function del<T>(url: string, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().delete<T>(url, { signal: config?.signal, headers: config?.headers })
  return response.data
}

// Call: DELETE request with body — rare but needed for bulk deletes
export async function delWithBody<T>(url: string, body?: unknown, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().delete<T>(url, { data: body, signal: config?.signal, headers: config?.headers })
  return response.data
}

// Call: GET request returning raw Blob — used for file downloads
export async function getBlob(url: string, config?: RequestConfig): Promise<Blob> {
  const response = await getApiClient().get<Blob>(url, { responseType: 'blob', signal: config?.signal, headers: config?.headers })
  return response.data
}
