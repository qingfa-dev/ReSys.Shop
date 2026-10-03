import type { ProductFilter } from '../index'

export interface ProductFilterParams {
  category?: string
  priceMin?: number
  priceMax?: number
  tags?: string[]
  inStock?: boolean
  sortBy?: 'newest' | 'price-asc' | 'price-desc' | 'popular'
  search?: string
  page?: number
  pageSize?: number
}

export function buildProductFilter(params: ProductFilter) {
  const result: Record<string, any> = {}

  if (params.optionTypeId && params.optionTypeId.length > 0) {
    result.optionValueId = params.optionTypeId
  }
  if (params.taxonId && params.taxonId.length > 0) {
    result.taxonId = params.taxonId
  }
  if (params.priceMin !== undefined) {
    result.minPrice = params.priceMin
  }
  if (params.priceMax !== undefined) {
    result.maxPrice = params.priceMax
  }
  if (params.tags && params.tags.length > 0) {
    result.tags = params.tags.join(',')
  }
  if (params.inStock) {
    result.inStock = true
  }
  if (params.sortBy) {
    result.sort = params.sortBy === 'newest' ? '-createdAtUtc'
      : params.sortBy === 'price-asc' ? 'price'
      : params.sortBy === 'price-desc' ? '-price'
      : 'name'
  }

  return result
}
