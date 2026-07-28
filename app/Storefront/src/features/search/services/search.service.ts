import type { Result, PagedResult } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'
import type { ISearchService } from './search.service.interface'
import type { ISearchRepository } from '../repositories/search.repository.interface'
import { searchApiRepository } from '../repositories/search.api'
import { searchMockRepository } from '../repositories/search.mock.repository'

const USE_MOCK = true

export class SearchService implements ISearchService {
  constructor(private readonly repository: ISearchRepository = USE_MOCK ? searchMockRepository : searchApiRepository) {}

  async search(query: string, filters?: Record<string, unknown>): Promise<PagedResult<Product>> {
    return this.repository.search(query, filters)
  }

  async getSuggestions(query: string): Promise<Result<string[]>> {
    return this.repository.getSuggestions(query)
  }
}

export const searchService = new SearchService()
