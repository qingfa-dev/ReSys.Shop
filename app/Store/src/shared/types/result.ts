import type { ApiError } from './error'

export interface Result<T> {
  isSuccess: boolean
  statusCode: number
  message: string | null
  errors: ApiError[]
  value: T
}

export interface PagedResult<T> {
  isSuccess: boolean
  statusCode: number
  message: string | null
  errors: ApiError[]
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export function ok<T>(value: T, statusCode = 200): Result<T> {
  return { isSuccess: true, statusCode, message: null, errors: [], value }
}

export function created<T>(value: T): Result<T> {
  return { isSuccess: true, statusCode: 201, message: null, errors: [], value }
}

export function noContent(): Result<null> {
  return { isSuccess: true, statusCode: 204, message: null, errors: [], value: null }
}

export function failure<T>(error: ApiError | string, statusCode = 400): Result<T> {
  const apiError: ApiError = typeof error === 'string'
    ? { code: 'Error', message: error, type: statusCode }
    : error
  return { isSuccess: false, statusCode, message: typeof error === 'string' ? error : apiError.message, errors: [apiError], value: null as T }
}

export function badRequest(message: string): Result<null> {
  return { isSuccess: false, statusCode: 400, message, errors: [{ code: 'BadRequest', message, type: 400 }], value: null }
}

export function notFound(message: string): Result<null> {
  return { isSuccess: false, statusCode: 404, message, errors: [{ code: 'NotFound', message, type: 404 }], value: null }
}

export function unauthorized(): Result<null> {
  return { isSuccess: false, statusCode: 401, message: 'Unauthorized', errors: [{ code: 'Unauthorized', message: 'Unauthorized', type: 401 }], value: null }
}

export function pagedOk<T>(items: T[], page: number, pageSize: number, totalCount: number, statusCode = 200): PagedResult<T> {
  return {
    isSuccess: true,
    statusCode,
    message: null,
    errors: [],
    items,
    page,
    pageSize,
    totalCount,
    totalPages: pageSize > 0 ? Math.ceil(totalCount / pageSize) : 0,
  }
}

export function pagedFailure<T>(errors: ApiError[], statusCode = 400): PagedResult<T> {
  return {
    isSuccess: false,
    statusCode,
    message: null,
    errors,
    items: [],
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  }
}

export function isSuccess<T>(result: Result<T>): boolean {
  return result.isSuccess
}

export function isFailure<T>(result: Result<T>): boolean {
  return !result.isSuccess
}

export function validation<T>(errors: ApiError[]): Result<T> {
  return { isSuccess: false, statusCode: 422, message: null, errors, value: null as T }
}
