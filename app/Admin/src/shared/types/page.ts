export interface PageRequest {
  page: number
  pageSize: number
  search?: string
}

export const DEFAULT_PAGE = 1
export const DEFAULT_PAGE_SIZE = 20
