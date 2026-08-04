import type { ApiProblemDetail } from './result'
import type { QueryingModel } from './querying'

export interface ApiError {
  statusCode: number
  message: string
  errors: ApiProblemDetail[]
}

export interface RequestOptions {
  query?: QueryingModel
  signal?: AbortSignal
  headers?: Record<string, string>
}
