/**
 * Pagination Models
 * Handles page offset, size, and result metadata.
 */

export interface PagingParams {
  page?: number
  pageSize?: number
}

export interface PageMeta {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export function createPagingParams(page = 1, pageSize = 10): PagingParams {
  return { page, pageSize }
}

export function createPaginationParams(page = 1, pageSize = 10): PagingParams {
  return { page, pageSize }
}

export function calculatePageMeta(totalCount: number, page: number, pageSize: number): PageMeta {
  const totalPages = pageSize > 0 ? Math.ceil(totalCount / pageSize) : 0
  return {
    page,
    pageSize,
    totalCount,
    totalPages,
    hasNextPage: page < totalPages,
    hasPreviousPage: page > 1,
  }
}