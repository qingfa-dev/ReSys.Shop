export const ErrorType = {
  BadRequest: 400,
  Unauthorized: 401,
  Forbidden: 403,
  NotFound: 404,
  Conflict: 409,
  Validation: 422,
  Unexpected: 500,
} as const

export type ErrorTypeValue = (typeof ErrorType)[keyof typeof ErrorType]

export interface ErrorModel {
  code: string
  message: string
  type: number
  metadata: Record<string, unknown> | null
}

export const ErrorConstant = {
  MaxCodeLength: 256,
  MaxMessageLength: 2048,
  DefaultCode: 'General.Unexpected',
  DefaultMessage: 'An unexpected error occurred.',
} as const
