import axios from 'axios'
import type { ApiError } from '@/shared/types/error'
import { HttpError } from '../errors'
import { notifyError } from '../notify'

// Normalize: Flatten a nested FluentValidation path (e.g. Request.Credential)
// to the camel-case form field name (e.g. credential).
function normalizeField(value: string): string {
  const segment = value.split('.').pop() ?? value
  return segment.length > 0 ? segment[0]!.toLowerCase() + segment.slice(1) : segment
}

// Transform: Normalize API error responses into typed ApiError array
function extractErrors(
  data: Record<string, unknown> | undefined,
  status: number,
): ApiError[] {
  // Check: Backend structured error envelope (validation errors array)
  if (data?.errors && Array.isArray(data.errors)) {
    return (data.errors as Array<{
      code: string
      message: string
      type?: number
      metadata?: Record<string, unknown>
    }>).map(e => {
      const raw = e.metadata?.['propertyName'] ?? e.metadata?.['fields'] ?? e.metadata?.['Field']
      return {
        code: e.code,
        message: e.message,
        type: e.type ?? status,
        field: typeof raw === 'string' && raw.length > 0 ? normalizeField(raw) : undefined,
      }
    })
  }

  // Check: Backend error-as-object format (title + code)
  if (typeof data?.title === 'string') {
    return [{ code: (data.code as string) ?? 'HttpError', message: data.title as string, type: status }]
  }

  // Fallback: Generic HTTP status message
  return [{ code: 'HttpError', message: `HTTP ${status}`, type: status }]
}

// Intercept: Transform Axios errors into HttpError — surface 5xx to user via toast
export async function errorInterceptor(error: unknown): Promise<never> {
  // Skip: Cancelled requests are not errors — propagate as-is
  if (axios.isCancel(error)) {
    return Promise.reject(error)
  }

  // Guard: Non-Axios errors wrapped as generic HttpError
  if (!axios.isAxiosError(error)) {
    return Promise.reject(new HttpError(0, [{ code: 'Unexpected', message: 'An unexpected error occurred.', type: 0 }]))
  }

  const status = error.response?.status ?? 0
  const data = error.response?.data as Record<string, unknown> | undefined

  // Skip: Result-wrapped responses (200 with isSuccess:false) — pass through for caller to handle
  // Guard: All other non-2xx responses are HTTP errors — reject with typed HttpError
  if (status >= 200 && status < 300 && data && 'isSuccess' in data) {
    return error.response as never
  }

  const errors = extractErrors(data, status)

  // Notify: Surface server errors to user immediately — client errors handled by caller
  if (status >= 500) {
    notifyError(errors[0]?.message ?? `HTTP ${status}`)
  }

  return Promise.reject(new HttpError(status, errors))
}
