export interface Error {
  code: string
  message: string
  type: number
  metadata: Record<string, unknown> | null
}

export interface Result<T> {
  isSuccess: boolean
  statusCode: number
  errors: Error[]
  message: string | null
  metadata: Record<string, unknown> | null
  value: T
}

export interface PagedResult<T> {
  isSuccess: boolean
  statusCode: number
  errors: Error[]
  message: string | null
  metadata: Record<string, unknown> | null
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}
