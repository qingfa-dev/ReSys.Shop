export interface PageRequest {
  page: number
  pageSize: number
  sort?: string
  direction?: 'asc' | 'desc'
  search?: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}
