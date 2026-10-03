import { z } from 'zod'

/** Validates API error type classification — maps to toast severity and error handling. */
export const ErrorTypeSchema = z.enum([
  'ValidationError',
  'NotFound',
  'Unauthorized',
  'Forbidden',
  'Conflict',
  'ServerError',
  'NetworkError',
])

/** Validates HTTP status codes as string literals — API returns status codes as strings. */
export const StatusCodeSchema = z.enum([
  '200', '201', '204', '400', '401', '403', '404', '409', '500',
])
