import type { QueryParams, QueryBuilderResult } from './types'
import { FilterBuilder } from './filter-builder'
import { SortBuilder } from './sort-builder'

export class QueryBuilder {
  private filterBuilder = new FilterBuilder()
  private sortBuilder = new SortBuilder()
  private searchTerm?: string
  private searchFields?: string[]
  private searchMode?: 'any' | 'all'
  private pageNumber?: number
  private pageSize?: number

  filterBy(action: (f: FilterBuilder) => void): this {
    this.filterBuilder = new FilterBuilder()
    action(this.filterBuilder)
    return this
  }

  sortBy(action: (s: SortBuilder) => void): this {
    this.sortBuilder = new SortBuilder()
    action(this.sortBuilder)
    return this
  }

  search(term: string, fields?: string[], mode?: 'any' | 'all'): this {
    this.searchTerm = term
    this.searchFields = fields
    this.searchMode = mode
    return this
  }

  page(page: number, pageSize: number): this {
    this.pageNumber = page
    this.pageSize = pageSize
    return this
  }

  build(): QueryBuilderResult {
    const params: QueryParams = {}

    const filter = this.filterBuilder.build()
    if (filter) params.filter = filter

    const sort = this.sortBuilder.build()
    if (sort) params.sort = sort

    if (this.searchTerm) {
      params.search = this.searchTerm
      if (this.searchFields && this.searchFields.length > 0) {
        params.searchFields = this.searchFields
      }
      if (this.searchMode) {
        params.searchMode = this.searchMode
      }
    }

    if (this.pageNumber !== undefined) {
      params.pageNumber = this.pageNumber
    }
    if (this.pageSize !== undefined) {
      params.pageSize = this.pageSize
    }

    const self = { params }

    return {
      params,
      toUrl(basePath: string): string {
        const search = new URLSearchParams()
        for (const [key, value] of Object.entries(self.params)) {
          if (Array.isArray(value)) {
            for (const v of value) {
              search.append(key, v)
            }
          } else if (value !== undefined && value !== null) {
            search.set(key, String(value))
          }
        }
        const qs = search.toString()
        return qs ? `${basePath}?${qs}` : basePath
      },
    }
  }
}
