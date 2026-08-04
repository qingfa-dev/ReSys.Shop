export type FilterOperator =
  | 'Equal'
  | 'EqualCaseSensitive'
  | 'NotEqual'
  | 'GreaterThan'
  | 'GreaterThanOrEqual'
  | 'LessThan'
  | 'LessThanOrEqual'
  | 'Contains'
  | 'ContainsCaseSensitive'
  | 'NotContains'
  | 'StartsWith'
  | 'StartsWithCaseSensitive'
  | 'NotStartsWith'
  | 'EndsWith'
  | 'EndsWithCaseSensitive'
  | 'NotEndsWith'

export type FilterLogic = 'And' | 'Or'

export interface FilterCondition {
  field: string
  operator: FilterOperator
  value: string
}

export interface FilterGroup {
  logic: FilterLogic
  conditions: FilterCondition[]
  groups: FilterGroup[]
}

export interface FilterModel {
  root?: FilterGroup
  conditions: FilterCondition[]
  allowedFields: string[]
  violations: string[]
}

export type SortDirection = 'Ascending' | 'Descending'

export type SortNulls = 'First' | 'Last'

export interface SortClause {
  field: string
  direction: SortDirection
  nulls?: SortNulls
}

export interface SortModel {
  clauses: SortClause[]
  allowedFields: string[]
  violations: string[]
}

export type SearchMode = 'Any' | 'All'

export interface SearchTerm {
  value: string
  caseSensitive: boolean
}

export interface SearchModel {
  term: SearchTerm
  fields: string[]
  mode: SearchMode
  allowedFields: string[]
  violations: string[]
}

export interface PageBounds {
  defaultPage: number
  defaultPageSize: number
  maxPageSize: number
}

export interface PageModel {
  page: number
  pageSize: number
  isEmpty: boolean
  bounds: PageBounds
  violations: string[]
}

export interface QueryingModel {
  filter: FilterModel
  search: SearchModel
  sort: SortModel
  page: PageModel
}

export function createDefaultQueryingModel(pageSize = 10): QueryingModel {
  return {
    filter: { conditions: [], allowedFields: [], violations: [] },
    search: {
      term: { value: '', caseSensitive: false },
      fields: [],
      mode: 'Any',
      allowedFields: [],
      violations: [],
    },
    sort: { clauses: [], allowedFields: [], violations: [] },
    page: {
      page: 1,
      pageSize,
      isEmpty: false,
      bounds: { defaultPage: 1, defaultPageSize: pageSize, maxPageSize: 100 },
      violations: [],
    },
  }
}
