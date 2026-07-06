import axios, { type AxiosInstance, type AxiosResponse, type AxiosError } from 'axios'
import { ApiError, fromAxiosError } from './errors'
import { getToken } from './fetch-options'
import type { Envelope } from './envelope'
import type { PagedResult } from './paged-result'

const BASE_URL = import.meta.env.VITE_API_URL || ''

const apiClient: AxiosInstance = axios.create({ baseURL: BASE_URL })

apiClient.interceptors.request.use((config) => {
  config.headers.set('Content-Type', 'application/json')
  config.headers.set('X-Request-Id', crypto.randomUUID())
  const token = getToken()
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})

apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error: AxiosError<Record<string, unknown> | undefined>) => {
    throw fromAxiosError(error)
  },
)

function unwrap<T>(envelope: Envelope<T>): T {
  if (!envelope.isSuccess) {
    const message = envelope.errors[0]?.message ?? 'Unknown error'
    throw new ApiError(422, message)
  }
  return envelope.value as T
}

async function request<T>(path: string, options?: { method?: string; body?: unknown }): Promise<T> {
  const response = await apiClient({
    url: path,
    method: options?.method ?? 'GET',
    data: options?.body ?? undefined,
    validateStatus: (status: number) => status >= 200 && status < 300,
  })
  if (response.status === 204) return undefined as T
  const data = response.data as T
  return data
}

export const api = {
  get<T>(path: string): Promise<T> {
    return request<Envelope<T> | T>(path).then((r) =>
      r && typeof r === 'object' && 'isSuccess' in r ? unwrap(r as Envelope<T>) : (r as T),
    )
  },
  getPaged<T>(path: string): Promise<PagedResult<T>> {
    return request<PagedResult<T>>(path)
  },
  post<T>(path: string, body?: unknown): Promise<T> {
    return request<Envelope<T> | T>(path, {
      method: 'POST',
      body,
    }).then((r) =>
      r && typeof r === 'object' && 'isSuccess' in r ? unwrap(r as Envelope<T>) : (r as T),
    )
  },
  put<T>(path: string, body?: unknown): Promise<T> {
    return request<Envelope<T> | T>(path, {
      method: 'PUT',
      body,
    }).then((r) =>
      r && typeof r === 'object' && 'isSuccess' in r ? unwrap(r as Envelope<T>) : (r as T),
    )
  },
  delete<T = void>(path: string): Promise<T> {
    return request<T>(path, { method: 'DELETE' })
  },
}

export { ApiError } from './errors'
