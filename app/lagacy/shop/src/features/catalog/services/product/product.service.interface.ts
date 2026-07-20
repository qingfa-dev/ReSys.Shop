import type { Result, PagedResult } from '@/core/models/result'
import type { Product, ProductFilter } from '../../types'

export interface IProductService {
  getProducts(filter?: ProductFilter, page?: number, pageSize?: number): Promise<PagedResult<Product>>
  getProduct(id: string): Promise<Result<Product>>
  getProductBySlug(slug: string): Promise<Result<Product>>
  searchProducts(query: string, limit?: number): Promise<Result<Product[]>>
  getFeaturedProducts(limit?: number): Promise<Result<Product[]>>
  getNewArrivals(limit?: number): Promise<Result<Product[]>>
}