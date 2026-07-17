import type { PaginationMeta, ServerError } from './result.types'

export interface SuccessResult<T> {
  data: T
  success: true
  meta?: PaginationMeta
}

export interface FailureResult {
  data: null
  success: false
  error: {
    statusCode: number
    title: string | null
    message: string | null
    detail: string | null
    errors: Record<string, string[]>
    error_code: string | undefined
  }
}

export type MappedResult<T> = SuccessResult<T> | FailureResult

export function mapToErrors(errors: ServerError[]): Record<string, string[]> {
  const result: Record<string, string[]> = {}
  for (const err of errors) {
    const key = err.code || 'general'
    if (!result[key]) result[key] = []
    result[key].push(err.message)
  }
  return result
}
