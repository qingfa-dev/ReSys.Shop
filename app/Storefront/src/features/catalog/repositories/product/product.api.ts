import { BaseRepository } from '@/core/repositories'
import type { Result, PagedResult } from '@/core/models/result'
import type { ProductResponse } from '../../types/response'
import type { IProductRepository } from './product.repository.interface'

export class ProductApiRepository extends BaseRepository implements IProductRepository {
  protected readonly endpoint = '/api/storefront/products'

  async getAll(params?: { paging?: { page: number; pageSize: number }; filter?: { filter: string }; search?: { search: string; searchFields: string[] }; sort?: { sortBy: string; sortOrder: 'asc' | 'desc' } }): Promise<PagedResult<ProductResponse>> {
    return super.getPaged<ProductResponse>(
      this.endpoint,
      params?.paging,
      params?.filter,
      params?.search,
      params?.sort
    )
  }

  getById<T = ProductResponse>(id: string): Promise<Result<T>> {
    return super.getById<T>(this.endpoint, id)
  }

  async getProductBySlug(slug: string): Promise<Result<ProductResponse>> {
    return this.get<ProductResponse>(`/api/storefront/products/slug/${slug}`)
  }

  async searchProducts(query: string, limit = 10): Promise<PagedResult<ProductResponse>> {
    return super.getPaged<ProductResponse>(
      '/api/storefront/products/search',
      { page: 1, pageSize: limit },
      undefined,
      { search: query, searchFields: ['name', 'description'] }
    )
  }

  async getFeaturedProducts(limit = 8): Promise<PagedResult<ProductResponse>> {
    return super.getPaged<ProductResponse>(
      '/api/storefront/products/featured',
      { page: 1, pageSize: limit }
    )
  }

  async getNewArrivals(limit = 8): Promise<PagedResult<ProductResponse>> {
    return super.getPaged<ProductResponse>(
      '/api/storefront/products/new',
      { page: 1, pageSize: limit }
    )
  }
}

export const productApiRepository = new ProductApiRepository()