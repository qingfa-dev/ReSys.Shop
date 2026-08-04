export interface ApiError {
  code: string
  message: string
  type: number
  metadata?: Record<string, unknown> | null
}

export const ErrorType = {
  BadRequest: 400,
  Unauthorized: 401,
  Forbidden: 403,
  NotFound: 404,
  Conflict: 409,
  Validation: 422,
  Unexpected: 500,
} as const
