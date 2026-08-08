import { z } from 'zod'

export const ErrorTypeSchema = z.enum([
  'ValidationError',
  'NotFound',
  'Unauthorized',
  'Forbidden',
  'Conflict',
  'ServerError',
  'NetworkError',
])

export const StatusCodeSchema = z.enum([
  '200', '201', '204', '400', '401', '403', '404', '409', '500',
])
