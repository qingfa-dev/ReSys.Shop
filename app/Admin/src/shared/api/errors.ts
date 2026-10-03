import type { ApiError } from '@/shared/types/error'

export class HttpError extends Error {
  statusCode: number
  errors: ApiError[]

  constructor(statusCode: number, errors: ApiError[]) {
    super(errors[0]?.message ?? 'Request failed.')
    this.name = 'HttpError'
    this.statusCode = statusCode
    this.errors = errors
  }
}
