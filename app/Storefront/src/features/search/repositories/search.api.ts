import { BaseRepository } from '@/core/repositories'
import type { Result, PagedResult } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'
import type { ISearchRepository } from './search.repository.interface'

export class SearchApiRepository extends BaseRepository implements ISearchRepository {
  async search(query: string, filters?: Record<string, unknown>): Promise<PagedResult<Product>> {
    return this.getPaged<Product>('/api/storefront/products?search=' + encodeURIComponent(query), undefined, filters as any)
  }

  async getSuggestions(_query: string): Promise<Result<string[]>> {
    // TODO: Backend endpoint /api/storefront/search/suggestions does not exist yet.
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Search suggestions endpoint not yet implemented' }
  }
}

export const searchApiRepository = new SearchApiRepository()
