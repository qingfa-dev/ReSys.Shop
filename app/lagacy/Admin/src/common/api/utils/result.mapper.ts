import type { ServerResult, ServerPagedResult, PaginationMeta, ServerError } from '../types/result.types'

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
    errorCode: string | undefined
  }
}

export type MappedResult<T> = SuccessResult<T> | FailureResult

export function isSuccess<T>(result: ServerResult<T> | ServerPagedResult<T>): boolean {
  return result.isSuccess
}

export function isFailure<T>(result: ServerResult<T> | ServerPagedResult<T>): boolean {
  return !result.isSuccess
}

export function toError(error: unknown): FailureResult {
  return {
    data: null,
    success: false,
    error: error as FailureResult['error'],
  }
}

export function mapToErrors(errors: ServerError[]): Record<string, string[]> {
  const result: Record<string, string[]> = {}
  for (const err of errors) {
    const key = err.code || 'general'
    if (!result[key]) result[key] = []
    result[key].push(err.message)
  }
  return result
}

export function resultToMapped<T>(result: ServerResult<T>): MappedResult<T> {
  if (result.isSuccess) {
    return { data: result.value, success: true as const }
  }
  return {
    data: null,
    success: false as const,
    error: {
      statusCode: result.statusCode,
      title: result.message,
      message: result.message,
      detail: null,
      errors: mapToErrors(result.errors),
      errorCode: result.errors[0]?.code,
    },
  }
}

export function pagedResultToMapped<T>(result: ServerPagedResult<T>): MappedResult<T[]> & { meta?: PaginationMeta } {
  if (result.isSuccess) {
    return {
      data: result.items,
      success: true as const,
      meta: {
        page: result.page,
        pageSize: result.pageSize,
        totalCount: result.totalCount,
        totalPages: Math.ceil(result.totalCount / result.pageSize),
      },
    }
  }
  return {
    data: null,
    success: false as const,
    error: {
      statusCode: result.statusCode,
      title: result.message,
      message: result.message,
      detail: null,
      errors: mapToErrors(result.errors),
      errorCode: result.errors[0]?.code,
    },
  }
}
