import axios from 'axios'
import type { AxiosError } from 'axios'
import type { ApiError } from '@/shared/types/error'
import { HttpError } from '../errors'

function extractErrors(
  data: Record<string, unknown> | undefined,
  status: number,
): ApiError[] {
  if (data?.errors && Array.isArray(data.errors)) {
    return (data.errors as Array<{ code: string; message: string; type?: number }>).map(e => ({
      code: e.code,
      message: e.message,
      type: e.type ?? status,
    }))
  }

  if (typeof data?.title === 'string') {
    return [{ code: (data.code as string) ?? 'HttpError', message: data.title as string, type: status }]
  }

  return [{ code: 'HttpError', message: `HTTP ${status}`, type: status }]
}

export async function errorInterceptor(error: unknown): Promise<never> {
  if (axios.isCancel(error)) {
    return Promise.reject(error)
  }

  if (!axios.isAxiosError(error)) {
    return Promise.reject(new HttpError(0, [{ code: 'Unexpected', message: 'An unexpected error occurred.', type: 0 }]))
  }

  const data = error.response?.data as Record<string, unknown> | undefined

  if (data && 'isSuccess' in data) {
    return error.response as never
  }

  const status = error.response?.status ?? 0
  const errors = extractErrors(data, status)
  return Promise.reject(new HttpError(status, errors))
}
