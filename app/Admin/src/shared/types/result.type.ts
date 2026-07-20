import type { ErrorModel } from './error.model'

export type ServerError = ErrorModel

export { type ErrorModel as Error }

export const ServerResultConstants = {
  Ok: 200,
  Created: 201,
  Accepted: 202,
  NoContent: 204,
} as const

export interface ServerResult<T> {
  isSuccess: boolean
  statusCode: number
  errors: ServerError[]
  message: string | null
  metadata: Record<string, unknown> | null
  value: T
}

export interface ServerPagedResult<T> {
  isSuccess: boolean
  statusCode: number
  errors: ServerError[]
  message: string | null
  metadata: Record<string, unknown> | null
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export function createServerResult<T>(
  statusCode: number,
  value: T,
  message?: string,
  metadata?: Record<string, unknown>,
): ServerResult<T> {
  return {
    isSuccess: statusCode < 400,
    statusCode,
    errors: [],
    message: message ?? null,
    metadata: metadata ?? null,
    value,
  }
}

export function createServerErrorResult<T>(
  statusCode: number,
  errors: ServerError[],
  message?: string,
  metadata?: Record<string, unknown>,
): ServerResult<T> {
  return {
    isSuccess: false,
    statusCode,
    errors,
    message: message ?? null,
    metadata: metadata ?? null,
    value: undefined as unknown as T,
  }
}

export function createServerPagedResult<T>(
  statusCode: number,
  items: T[],
  page: number,
  pageSize: number,
  totalCount: number,
  message?: string,
  metadata?: Record<string, unknown>,
): ServerPagedResult<T> {
  return {
    isSuccess: statusCode < 400,
    statusCode,
    errors: [],
    message: message ?? null,
    metadata: metadata ?? null,
    items,
    page,
    pageSize,
    totalCount,
  }
}
