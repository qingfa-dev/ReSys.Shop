import { getApiClient } from './axios'
import { STORAGE_KEYS } from '@/shared/constants/storage'
export { HttpError } from './errors'

export function setBaseUrl(url: string): void {
  getApiClient().defaults.baseURL = url
}

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

export async function get<T>(url: string, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().get<T>(url, { signal: config?.signal, headers: config?.headers })
  return response.data
}

function requestConfig(body?: unknown, config?: RequestConfig): { signal?: AbortSignal; headers?: Record<string, string | undefined> } {
  const headers: Record<string, string | undefined> = { ...config?.headers }
  if (body instanceof FormData) {
    headers['Content-Type'] = undefined
  }
  return { signal: config?.signal, headers }
}

export async function post<T>(url: string, body?: unknown, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().post<T>(url, body, requestConfig(body, config))
  return response.data
}

export async function put<T>(url: string, body?: unknown, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().put<T>(url, body, requestConfig(body, config))
  return response.data
}

export async function patch<T>(url: string, body?: unknown, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().patch<T>(url, body, requestConfig(body, config))
  return response.data
}

export async function del<T>(url: string, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().delete<T>(url, { signal: config?.signal, headers: config?.headers })
  return response.data
}

export async function delWithBody<T>(url: string, body?: unknown, config?: RequestConfig): Promise<T> {
  const response = await getApiClient().delete<T>(url, { data: body, signal: config?.signal, headers: config?.headers })
  return response.data
}

export async function getBlob(url: string, config?: RequestConfig): Promise<Blob> {
  const response = await getApiClient().get<Blob>(url, { responseType: 'blob', signal: config?.signal, headers: config?.headers })
  return response.data
}
