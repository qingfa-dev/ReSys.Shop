export type FilterOperator =
  | 'eq' | 'neq'
  | 'gt' | 'gte' | 'lt' | 'lte'
  | 'contains' | 'starts' | 'ends'

export type FilterLogic = 'and' | 'or'

export interface FilterClause {
  field: string
  operator: FilterOperator
  value: string
  logic: FilterLogic
  caseSensitive?: boolean
}

export interface SortClause {
  field: string
  direction: 'asc' | 'desc'
}

export interface QueryParams {
  filter?: string
  search?: string
  searchFields?: string[]
  searchMode?: 'any' | 'all'
  sort?: string[]
  pageNumber?: number
  pageSize?: number
}

export interface QueryBuilderResult {
  params: QueryParams
  toUrl(basePath: string): string
}
