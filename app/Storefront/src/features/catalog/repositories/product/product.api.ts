import { BaseRepository } from '@/core/repositories'
import type { Result, PagedResult } from '@/core/models/result'
import type { ProductResponse } from '../../types/response'
import type { IProductRepository } from './product.repository.interface'

export class ProductApiRepository extends BaseRepository implements IProductRepository {
  protected readonly endpoint = '/api/storefront/products'

  async getAll(filter?: Record<string, any>): Promise<PagedResult<ProductResponse>> {
    const searchParams = new URLSearchParams()

    if (filter) {
      for (const [key, value] of Object.entries(filter)) {
        if (value === undefined || value === null) continue
        if (Array.isArray(value)) {
          for (const item of value) {
            searchParams.append(key, String(item))
          }
        } else {
          searchParams.append(key, String(value))
        }
      }
    }

    const queryString = searchParams.toString()
    const response = await this.client.get<PagedResult<ProductResponse>>(
      `${this.endpoint}${queryString ? `?${queryString}` : ''}`
    )

    return response.data
  }

  getById<T = ProductResponse>(id: string): Promise<Result<T>> {
    return super.getById<T>(this.endpoint, id)
  }

  async getProductBySlug(slug: string): Promise<Result<ProductResponse>> {
    return this.get<ProductResponse>(`/api/storefront/products/${slug}`)
  }
}

export const productApiRepository = new ProductApiRepository()