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

export async function get<T>(url: string, signal?: AbortSignal): Promise<T> {
  const response = await getApiClient().get<T>(url, { signal })
  return response.data
}

export async function post<T>(url: string, body?: unknown, signal?: AbortSignal): Promise<T> {
  const response = await getApiClient().post<T>(url, body, { signal })
  return response.data
}

export async function put<T>(url: string, body?: unknown, signal?: AbortSignal): Promise<T> {
  const response = await getApiClient().put<T>(url, body, { signal })
  return response.data
}

export async function patch<T>(url: string, body?: unknown, signal?: AbortSignal): Promise<T> {
  const response = await getApiClient().patch<T>(url, body, { signal })
  return response.data
}

export async function del<T>(url: string, signal?: AbortSignal): Promise<T> {
  const response = await getApiClient().delete<T>(url, { signal })
  return response.data
}

export async function delWithBody<T>(url: string, body?: unknown, signal?: AbortSignal): Promise<T> {
  const response = await getApiClient().delete<T>(url, { data: body, signal })
  return response.data
}
