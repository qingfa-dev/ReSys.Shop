import { mockCategoryRepository } from '../../repositories/category/category.mock.repository'
import { categoryApiRepository } from '../../repositories/category/category.api'
import type { ICategoryService } from './category.service.interface'
import type { Category } from '../../types'
import type { Result } from '@/core/models/result'
import { succeed, fail } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class CategoryService implements ICategoryService {
  private readonly categoryRepository = USE_MOCK ? mockCategoryRepository : categoryApiRepository

  async getCategories(): Promise<Result<Category[]>> {
    const response = await this.categoryRepository.getAll()
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to get categories', response.statusCode, response.errors)
    }
    return succeed(response.items.map(c => ({
      id: c.id,
      name: c.name,
      slug: c.slug,
      parentId: c.parentId,
      image: c.image,
    })), response.statusCode)
  }
}

export const categoryService = new CategoryService()