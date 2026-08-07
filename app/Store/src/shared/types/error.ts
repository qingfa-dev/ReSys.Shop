import type { Result } from './result'

export enum StatusCode {
  Ok = 200,
  Created = 201,
  NoContent = 204,
  BadRequest = 400,
  Unauthorized = 401,
  Forbidden = 403,
  NotFound = 404,
  Conflict = 409,
  Validation = 422,
  TooManyRequests = 429,
  InternalServerError = 500,
}

export enum ErrorType {
  BadRequest = 400,
  Unauthorized = 401,
  Forbidden = 403,
  NotFound = 404,
  Conflict = 409,
  Validation = 422,
  Unexpected = 500,
}

export interface ApiError {
  code: string
  message: string
  type: number
  metadata?: Record<string, unknown> | null
}
