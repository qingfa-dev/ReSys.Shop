export const FilterOperator = {
  Equal: 'Equal',
  EqualCaseSensitive: 'EqualCaseSensitive',
  NotEqual: 'NotEqual',
  GreaterThan: 'GreaterThan',
  GreaterThanOrEqual: 'GreaterThanOrEqual',
  LessThan: 'LessThan',
  LessThanOrEqual: 'LessThanOrEqual',
  Contains: 'Contains',
  ContainsCaseSensitive: 'ContainsCaseSensitive',
  NotContains: 'NotContains',
  StartsWith: 'StartsWith',
  StartsWithCaseSensitive: 'StartsWithCaseSensitive',
  NotStartsWith: 'NotStartsWith',
  EndsWith: 'EndsWith',
  EndsWithCaseSensitive: 'EndsWithCaseSensitive',
  NotEndsWith: 'NotEndsWith',
} as const

export type FilterOperator = (typeof FilterOperator)[keyof typeof FilterOperator]

export const FilterLogic = {
  And: 'And',
  Or: 'Or',
} as const

export type FilterLogic = (typeof FilterLogic)[keyof typeof FilterLogic]

export const SearchMode = {
  Any: 'Any',
  All: 'All',
} as const

export type SearchMode = (typeof SearchMode)[keyof typeof SearchMode]

export const SortDirection = {
  Ascending: 'Ascending',
  Descending: 'Descending',
} as const

export type SortDirection = (typeof SortDirection)[keyof typeof SortDirection]

export const SortNulls = {
  First: 'First',
  Last: 'Last',
} as const

export type SortNulls = (typeof SortNulls)[keyof typeof SortNulls]
