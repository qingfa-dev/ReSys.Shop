/** HTTP error wrapping the server status code and message. */
export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

const BASE_URL = import.meta.env.VITE_API_URL || ''

/** Base request helper. Throws {@link ApiError} on non-2xx responses. */
async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const url = `${BASE_URL}${path}`
  const response = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  })
  if (!response.ok) {
    throw new ApiError(response.status, `API error: ${response.statusText}`)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export const api = {
  /** Send a GET request. */
  get<T>(path: string): Promise<T> {
    return request<T>(path)
  },
  /** Send a POST request with an optional JSON body. */
  post<T>(path: string, body?: unknown): Promise<T> {
    return request<T>(path, { method: 'POST', body: body ? JSON.stringify(body) : undefined })
  },
  /** Send a PUT request with an optional JSON body. */
  put<T>(path: string, body?: unknown): Promise<T> {
    return request<T>(path, { method: 'PUT', body: body ? JSON.stringify(body) : undefined })
  },
  /** Send a DELETE request. */
  delete<T = void>(path: string): Promise<T> {
    return request<T>(path, { method: 'DELETE' })
  },
}
