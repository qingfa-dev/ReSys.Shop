import type { Result, PagedResult } from '@/core/models/result'
import type { ProductResponse } from '../../types/response'

export interface IProductRepository {
  getAll(params?: Record<string, any>): Promise<PagedResult<ProductResponse>>
  getById<T = ProductResponse>(id: string): Promise<Result<T>>
  getProductBySlug(slug: string): Promise<Result<ProductResponse>>

}