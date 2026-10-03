import type { Result, PagedResult } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'

export interface ISearchRepository {
  search(query: string, filters?: Record<string, unknown>): Promise<PagedResult<Product>>
  getSuggestions(query: string): Promise<Result<string[]>>
}
