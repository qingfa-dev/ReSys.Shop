export interface PagingParams {
  page?: number
  pageSize?: number
}

export type SortDirection = 'asc' | 'desc'

export interface SortClause {
  field: string
  direction?: SortDirection
  nulls?: 'first' | 'last'
}

export interface SortParams {
  sort?: string[]
}

export type SearchMode = 'any' | 'all'

export interface SearchParams {
  search?: string
  searchFields?: string[]
  searchMode?: SearchMode
}

export interface FilterParams {
  filter?: string
}

export interface ServerQueryingParameters extends PagingParams, SortParams, SearchParams, FilterParams {}

/** @deprecated Use `ServerQueryingParameters` instead */
export type QueryParams = ServerQueryingParameters
