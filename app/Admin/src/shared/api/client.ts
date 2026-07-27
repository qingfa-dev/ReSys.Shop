import { getApiClient } from './axios'
export { HttpError } from './errors'

export function setBaseUrl(url: string): void {
  getApiClient().defaults.baseURL = url
}

export function setAuthToken(token: string | null): void {
  if (token) {
    localStorage.setItem('accessToken', token)
  } else {
    localStorage.removeItem('accessToken')
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
