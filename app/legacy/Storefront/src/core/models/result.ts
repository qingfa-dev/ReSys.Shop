export interface Result<T> {
  isSuccess: boolean
  isFailure: boolean
  statusCode: number
  message?: string
  data?: T
  errors?: Array<{
    code: string
    description: string
    field?: string
  }>
}

export interface PagedResult<T> {
  isSuccess: boolean
  isFailure: boolean
  statusCode: number
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
  message?: string
  errors?: Array<{
    code: string
    description: string
    field?: string
  }>
}

export interface ResultHelpers {
  success<T>(data: T, statusCode?: number): Result<T>
  failure<T>(message: string, statusCode?: number, errors?: Result<T>['errors']): Result<T>
}

export const resultHelpers: ResultHelpers = {
  success<T>(data: T, statusCode = 200): Result<T> {
    return {
      isSuccess: true,
      isFailure: false,
      statusCode,
      data,
    }
  },
  failure<T>(message: string, statusCode = 400, errors?: Result<T>['errors']): Result<T> {
    return {
      isSuccess: false,
      isFailure: true,
      statusCode,
      message,
      errors,
    }
  },
}

export function isResultSuccess<T>(result: Result<T>): boolean {
  return result.isSuccess === true
}

export function isResultFailure<T>(result: Result<T>): boolean {
  return result.isFailure === true
}
