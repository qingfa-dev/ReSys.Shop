import type { CategoryResponse, CategoryListResponse, CategorySingleResponse } from '../../types/response'
import type { PagingParams } from '@/core/models'
import { mockCategories, getCategoryById, getCategoryBySlug } from '../../data/mock-categories.data'
import { paginateResults } from '@/core/helpers/mock-query.helper'

export interface CategoryQueryParams {
  paging?: PagingParams
}

function mapToCategoryResponse(category: typeof mockCategories[0]): CategoryResponse {
  return {
    id: category.id,
    name: category.name,
    slug: category.slug,
    parentId: category.parentId,
    image: category.image,
  }
}

export class MockCategoryRepository {
  async getAll(params?: CategoryQueryParams): Promise<CategoryListResponse> {
    const page = params?.paging?.page ?? 1
    const pageSize = params?.paging?.pageSize ?? 10
    const result = mockCategories.map(mapToCategoryResponse)
    const { items, meta } = paginateResults(result, page, pageSize)
    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      items,
      page: meta.page,
      pageSize: meta.pageSize,
      totalCount: meta.totalCount,
      totalPages: meta.totalPages,
      hasNextPage: meta.hasNextPage,
      hasPreviousPage: meta.hasPreviousPage,
    }
  }

  async getById(id: string): Promise<CategorySingleResponse> {
    const category = getCategoryById(id)
    if (!category) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Category not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCategoryResponse(category) }
  }

  async getBySlug(slug: string): Promise<CategorySingleResponse> {
    const category = getCategoryBySlug(slug)
    if (!category) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Category not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCategoryResponse(category) }
  }
}

export const mockCategoryRepository = new MockCategoryRepository()