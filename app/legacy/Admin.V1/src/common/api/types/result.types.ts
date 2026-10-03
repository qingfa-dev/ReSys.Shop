export enum ErrorType {
  Validation = 0,
  NotFound = 1,
  Conflict = 2,
  UnprocessableEntity = 3,
  InternalServerError = 4,
}

export interface ServerError {
  code: string
  message: string
  type: number
  metadata: Record<string, unknown> | null
}

export interface ServerResult<T> {
  isSuccess: boolean
  statusCode: number
  errors: ServerError[]
  message: string | null
  metadata: Record<string, unknown> | null
  value: T
}

export interface ServerPagedResult<T> {
  isSuccess: boolean
  statusCode: number
  errors: ServerError[]
  message: string | null
  metadata: Record<string, unknown> | null
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface PaginationMeta {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
