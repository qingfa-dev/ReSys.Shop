import type { ApiError } from './error'

export interface Result<T> {
  isSuccess: boolean
  statusCode: number
  message: string | null
  errors: ApiError[]
  value: T
}

export interface PagedResult<T> {
  isSuccess: boolean
  statusCode: number
  message: string | null
  errors: ApiError[]
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
