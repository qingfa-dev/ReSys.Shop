import { BaseRepository } from '@/core/repositories'
import type { Result, PagedResult } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'
import type { ISearchRepository } from './search.repository.interface'

export class SearchApiRepository extends BaseRepository implements ISearchRepository {
  async search(query: string, filters?: Record<string, unknown>): Promise<PagedResult<Product>> {
    return this.getPaged<Product>('/api/storefront/products?search=' + encodeURIComponent(query), undefined, filters as any)
  }

  async getSuggestions(query: string): Promise<Result<string[]>> {
    return this.get<string[]>('/api/storefront/search/suggestions', { q: query })
  }
}

export const searchApiRepository = new SearchApiRepository()
