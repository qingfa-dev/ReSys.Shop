import type { Result, PagedResult } from '@/core/models/result'
import type { ProductResponse } from '../../types/response'

export interface ProductQueryParams {
  paging?: { page: number; pageSize: number }
  filter?: { filter: string }
  search?: { search: string; searchFields: string[] }
  sort?: { sortBy: string; sortOrder: 'asc' | 'desc' }
}

export interface IProductRepository {
  getAll(params?: ProductQueryParams): Promise<PagedResult<ProductResponse>>
  getById<T = ProductResponse>(id: string): Promise<Result<T>>
  getProductBySlug(slug: string): Promise<Result<ProductResponse>>
  searchProducts(query: string, limit?: number): Promise<PagedResult<ProductResponse>>
  getFeaturedProducts(limit?: number): Promise<PagedResult<ProductResponse>>
  getNewArrivals(limit?: number): Promise<PagedResult<ProductResponse>>
}