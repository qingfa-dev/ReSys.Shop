import type { Result } from '@/core/models/result'
import type { Category } from '../../types'

export interface ICategoryService {
  getCategories(): Promise<Result<Category[]>>
}