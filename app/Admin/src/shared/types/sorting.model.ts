export type SortDirection = 'asc' | 'desc'

export type SortNulls = 'first' | 'last'

export interface SortClause {
  field: string
  direction: SortDirection
  nulls?: SortNulls
}

export interface SortModel {
  clauses: SortClause[]
  allowedFields?: string[]
  isValid: boolean
  violations: string[]
  isEmpty: boolean
}

export const emptySortModel: SortModel = Object.freeze({
  clauses: [],
  isValid: true,
  violations: [],
  isEmpty: true,
})
