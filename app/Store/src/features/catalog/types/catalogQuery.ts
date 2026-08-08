import type { QueryingParameters } from '@/shared/types/querying'

export interface CatalogFilterParams {
  searchQuery?: string
  taxonIds?: string[]
  optionValueIds?: string[]
  minPrice?: number
  maxPrice?: number
}

export interface ProductQuery extends CatalogFilterParams {
  pageNumber?: number
  pageSize?: number
  search?: string
  filter?: string
  sort?: string[]
}

export function toProductQueryParams(q: ProductQuery): QueryingParameters {
  // Transform: Strip undefined fields to avoid sending empty params to API
  const params: QueryingParameters = {}
  if (q.pageNumber) params.pageNumber = q.pageNumber
  if (q.pageSize) params.pageSize = q.pageSize
  if (q.search) params.search = q.search
  if (q.filter) params.filter = q.filter
  if (q.sort) params.sort = q.sort
  // Map: Dedicated storefront filters — camelCase names match GetStorefrontProducts.Parameters
  if (q.taxonIds?.length) params.taxonId = q.taxonIds
  if (q.optionValueIds?.length) params.optionValueId = q.optionValueIds
  if (q.minPrice != null) params.minPrice = q.minPrice
  if (q.maxPrice != null) params.maxPrice = q.maxPrice
  return params
}
