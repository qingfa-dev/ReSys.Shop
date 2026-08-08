export interface CatalogFilterParams {
  searchQuery?: string
  taxonIds?: string[]
  optionValueIds?: string[]
  minPrice?: number
  maxPrice?: number
}

export interface ProductQuery {
  pageNumber?: number
  pageSize?: number
  search?: string
  filter?: string
  sort?: string[]
}

export function toProductQueryParams(q: ProductQuery): Record<string, unknown> {
  // Transform: Strip undefined fields to avoid sending empty params to API
  const params: Record<string, unknown> = {}
  if (q.pageNumber) params.pageNumber = q.pageNumber
  if (q.pageSize) params.pageSize = q.pageSize
  if (q.search) params.search = q.search
  if (q.filter) params.filter = q.filter
  if (q.sort) params.sort = q.sort
  return params
}
