import type { Error } from './result'
import type { QueryingModel } from './querying'

export interface ApiError {
  statusCode: number
  message: string
  errors: Error[]
}

export interface RequestOptions {
  query?: QueryingModel
  signal?: AbortSignal
  headers?: Record<string, string>
}
