import { ApiError } from './errors'
import { buildHeaders } from './fetch-options'
import type { Envelope } from './envelope'
import type { PagedResult } from './paged-result'

const BASE_URL = import.meta.env.VITE_API_URL || ''

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: buildHeaders(options?.headers),
  })
  if (!response.ok) {
    const message = await response.text().catch(() => response.statusText)
    throw new ApiError(response.status, message || `API error: ${response.statusText}`)
  }
  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

function unwrap<T>(envelope: Envelope<T>): T {
  if (!envelope.isSuccess) {
    const message = envelope.errors[0]?.message ?? 'Unknown error'
    throw new ApiError(422, message)
  }
  return envelope.value as T
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
      body: body ? JSON.stringify(body) : undefined,
    }).then((r) =>
      r && typeof r === 'object' && 'isSuccess' in r ? unwrap(r as Envelope<T>) : (r as T),
    )
  },
  put<T>(path: string, body?: unknown): Promise<T> {
    return request<Envelope<T> | T>(path, {
      method: 'PUT',
      body: body ? JSON.stringify(body) : undefined,
    }).then((r) =>
      r && typeof r === 'object' && 'isSuccess' in r ? unwrap(r as Envelope<T>) : (r as T),
    )
  },
  delete<T = void>(path: string): Promise<T> {
    return request<T>(path, { method: 'DELETE' })
  },
}

export { ApiError } from './errors'
