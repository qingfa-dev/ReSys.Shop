import type { FilterGroup, SortClause } from './querying'

export interface ListQuery {
  page: number
  pageSize: number
  search?: {
    value: string
    fields?: string[]
    mode?: 'Any' | 'All'
    caseSensitive?: boolean
  }
  sort?: SortClause[]
  filters?: FilterGroup
}

export function defaultListQuery(pageSize = 20): ListQuery {
  return { page: 1, pageSize, sort: [{ field: 'createdAt', direction: 'Descending' }] }
}
