export { type PageRequest } from '@/shared/types/page'

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}
