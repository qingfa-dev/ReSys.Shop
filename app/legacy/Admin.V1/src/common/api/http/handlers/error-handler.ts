import type { ServerError } from '../../types/result.types'
import { toCamelCaseKeys } from '@/common/mapper/mapper.utils'

export interface ParsedApiError {
  statusCode: number
  title: string | null
  message: string | null
  detail: string | null
  isSuccess: boolean
  errors: Record<string, string[]>
  errorCode: string | undefined
}

function convertServerErrors(errors: unknown): Record<string, string[]> {
  if (!errors) return {}

  if (Array.isArray(errors)) {
    const se = errors as ServerError[]
    if (se.length > 0 && se[0]?.code !== undefined) {
      const result: Record<string, string[]> = {}
      for (const err of se) {
        const key = err.code || 'general'
        if (!result[key]) result[key] = []
        result[key].push(err.message)
      }
      return result
    }
    return {}
  }

  return errors as Record<string, string[]>
}

export function parseApiError(error: unknown): ParsedApiError {
  if (!error || typeof error !== 'object') {
    return {
      statusCode: 500,
      title: 'Connection Error',
      message: null,
      detail: 'An unexpected error occurred.',
      isSuccess: false,
      errors: {},
      errorCode: undefined,
    }
  }

  const axiosError = error as {
    isAxiosError?: boolean
    response?: { data?: Record<string, unknown>; status?: number }
    request?: unknown
    message?: string
  }

  if (axiosError.isAxiosError || axiosError.response || axiosError.request) {
    const apiData = axiosError.response?.data

    if (apiData && typeof apiData === 'object') {
      const data = toCamelCaseKeys(apiData as Record<string, unknown>)

      const statusCode = (data.statusCode ?? data.status ?? axiosError.response?.status) as number | undefined
      const message = data.message as string | undefined
      const isSuccess = data.isSuccess as boolean | undefined
      const rawErrors = data.errors

      const title = (data.title ?? message) as string | undefined
      const detail = (data.detail ?? message) as string | undefined
      const errorCode = data.errorCode as string | undefined
      const resolvedCode = statusCode ?? 500

      return {
        statusCode: resolvedCode,
        title: title ?? (resolvedCode >= 500 ? 'Server Error' : 'Request Error'),
        message: message ?? title ?? null,
        detail: detail ?? null,
        isSuccess: isSuccess ?? false,
        errors: convertServerErrors(rawErrors),
        errorCode: errorCode,
      }
    }

    if (axiosError.request && !axiosError.response) {
      return {
        statusCode: 500,
        title: 'Connection Error',
        message: null,
        detail: axiosError.message || 'Network Error. Please check your internet connection.',
        isSuccess: false,
        errors: {},
        errorCode: undefined,
      }
    }
  }

  const e = error as Record<string, unknown>
  if (e.status !== undefined || e.statusCode !== undefined) {
    const rawErrors = e.errors ?? e.Errors
    return {
      statusCode: (e.statusCode ?? e.status ?? 500) as number,
      title: ((e.title ?? e.message) as string | undefined) ?? null,
      message: ((e.message ?? e.title) as string | undefined) ?? null,
      detail: (e.detail as string | undefined) ?? null,
      isSuccess: (e.isSuccess ?? false) as boolean,
      errors: convertServerErrors(rawErrors),
      errorCode: (e.error_code ?? e.errorCode) as string | undefined,
    }
  }

  return {
    statusCode: 500,
    title: null,
    message: null,
    detail: null,
    isSuccess: false,
    errors: {},
    errorCode: undefined,
  }
}
