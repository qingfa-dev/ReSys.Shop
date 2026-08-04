import type { ApiError } from './error'

export const StatusCode = {
  Ok: 200,
  Created: 201,
  Accepted: 202,
  NoContent: 204,
  BadRequest: 400,
  Unauthorized: 401,
  Forbidden: 403,
  NotFound: 404,
  Conflict: 409,
  Gone: 410,
  UnprocessableEntity: 422,
  TooManyRequests: 429,
  InternalServerError: 500,
  NotImplemented: 501,
  BadGateway: 502,
  ServiceUnavailable: 503,
  GatewayTimeout: 504,
} as const

export interface Result<T> {
  isSuccess: boolean
  statusCode: number
  message: string | null
  errors: ApiError[]
  metadata: Record<string, unknown> | null
  value: T
}

export interface PagedResult<T> {
  isSuccess: boolean
  statusCode: number
  message: string | null
  errors: ApiError[]
  metadata: Record<string, unknown> | null
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export function pagedOk<T>(
  items: T[],
  page: number,
  pageSize: number,
  totalCount: number,
): PagedResult<T> {
  return {
    isSuccess: true,
    statusCode: StatusCode.Ok,
    message: null,
    errors: [],
    metadata: null,
    items,
    page,
    pageSize,
    totalCount,
    totalPages: pageSize <= 0 ? 0 : Math.ceil(totalCount / pageSize),
  }
}

export function pagedFailure<T>(
  errors: ApiError[],
  statusCode: number = StatusCode.InternalServerError,
  message?: string,
): PagedResult<T> {
  return {
    isSuccess: false,
    statusCode,
    message: message ?? errors[0]?.message ?? 'Request failed.',
    errors,
    metadata: null,
    items: [],
    page: 1,
    pageSize: 0,
    totalCount: 0,
    totalPages: 0,
  }
}

export function isSuccess<T>(result: Result<T> | PagedResult<T>): boolean {
  return result.isSuccess
}

export function isFailure<T>(result: Result<T> | PagedResult<T>): boolean {
  return !result.isSuccess
}

export function ok<T>(value: T): Result<T> {
  return {
    isSuccess: true,
    statusCode: StatusCode.Ok,
    message: 'Operation completed successfully.',
    errors: [],
    metadata: null,
    value,
  }
}

export function created<T>(value: T): Result<T> {
  return {
    isSuccess: true,
    statusCode: StatusCode.Created,
    message: 'Resource created successfully.',
    errors: [],
    metadata: null,
    value,
  }
}

export function noContent(): Result<null> {
  return {
    isSuccess: true,
    statusCode: StatusCode.NoContent,
    message: null,
    errors: [],
    metadata: null,
    value: null,
  }
}

export function failure<T>(error: ApiError): Result<T> {
  return {
    isSuccess: false,
    statusCode: error.type,
    message: error.message,
    errors: [error],
    metadata: null,
    value: null as unknown as T,
  }
}

export function badRequest<T>(message: string, code = 'BadRequest'): Result<T> {
  return failure<T>({ code, message, type: StatusCode.BadRequest })
}

export function notFound<T>(message: string, code = 'NotFound'): Result<T> {
  return failure<T>({ code, message, type: StatusCode.NotFound })
}

export function unauthorized<T>(message = 'Authentication required.'): Result<T> {
  return failure<T>({ code: 'Unauthorized', message, type: StatusCode.Unauthorized })
}

export function forbidden<T>(message = 'Access denied.'): Result<T> {
  return failure<T>({ code: 'Forbidden', message, type: StatusCode.Forbidden })
}

export function conflict<T>(message: string, code = 'Conflict'): Result<T> {
  return failure<T>({ code, message, type: StatusCode.Conflict })
}

export function validation<T>(errors: ApiError[]): Result<T> {
  return {
    isSuccess: false,
    statusCode: StatusCode.UnprocessableEntity,
    message: 'Validation failed.',
    errors,
    metadata: null,
    value: null as unknown as T,
  }
}

export function unexpected<T>(message = 'An unexpected error occurred.'): Result<T> {
  return failure<T>({ code: 'Unexpected', message, type: StatusCode.InternalServerError })
}
