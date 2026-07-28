import { BaseRepository } from '@/core/repositories'
import type { Result, PagedResult } from '@/core/models/result'
import type { CategoryResponse } from '../../types/response'
import type { ICategoryRepository } from './category.repository.interface'

export class CategoryApiRepository extends BaseRepository implements ICategoryRepository {
  protected readonly endpoint = '/api/storefront/taxonomies'

  async getAll(params?: { page?: number; pageSize?: number }): Promise<PagedResult<CategoryResponse>> {
    return super.getPaged<CategoryResponse>(this.endpoint, params)
  }

  getById<T = CategoryResponse>(id: string): Promise<Result<T>> {
    return super.getById<T>(this.endpoint, id)
  }

}

export const categoryApiRepository = new CategoryApiRepository()
