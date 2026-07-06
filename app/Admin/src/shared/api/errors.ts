export const ErrorCode = {
  BadRequest: 400,
  Unauthorized: 401,
  Forbidden: 403,
  NotFound: 404,
  Conflict: 409,
  Validation: 422,
  Server: 500,
} as const
export type ErrorCodeValue = (typeof ErrorCode)[keyof typeof ErrorCode]

export class ApiError extends Error {
  public readonly code: ErrorCodeValue

  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
    this.code = (status as ErrorCodeValue) ?? ErrorCode.Server
  }
}

export function isApiError(value: unknown): value is ApiError {
  return value instanceof ApiError
}
