import { queryBuilder } from '@/core/helpers/query.builder'
import type { ProductEntity } from '../entity'

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

export function buildProductFilter(params: ProductFilterParams) {
  const builder = queryBuilder<ProductEntity>()

  if (params.category) {
    builder.where('category.slug', '=', params.category)
  }
  if (params.priceMin !== undefined) {
    builder.where('price', '>=', params.priceMin)
  }
  if (params.priceMax !== undefined) {
    builder.where('price', '<=', params.priceMax)
  }
  if (params.tags && params.tags.length > 0) {
    params.tags.forEach((tag) => {
      builder.where('tags', '*', tag)
    })
  }
  if (params.inStock) {
    builder.where('inventory.quantity', '>', 0)
  }
  if (params.search) {
    builder.search(params.search, ['name', 'description', 'category.name'])
  }

  if (params.sortBy) {
    switch (params.sortBy) {
      case 'newest':
        builder.orderBy('createdAt', 'desc')
        break
      case 'price-asc':
        builder.orderBy('price', 'asc')
        break
      case 'price-desc':
        builder.orderBy('price', 'desc')
        break
      case 'popular':
        builder.orderBy('name', 'asc')
        break
    }
  }

  if (params.page !== undefined && params.pageSize !== undefined) {
    builder.page(params.page, params.pageSize)
  }

  return builder.build()
}