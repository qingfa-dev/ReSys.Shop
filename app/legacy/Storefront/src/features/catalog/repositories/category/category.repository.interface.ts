import type { Result, PagedResult } from '@/core/models/result'
import type { CategoryResponse } from '../../types/response'

export interface ICategoryRepository {
  getAll(params?: { page?: number; pageSize?: number }): Promise<PagedResult<CategoryResponse>>
  getById<T = CategoryResponse>(id: string): Promise<Result<T>>

}
