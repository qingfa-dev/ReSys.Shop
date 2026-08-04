import type { SortDirection, SortNulls } from './enums'

export interface SortClause {
  field: string
  direction: SortDirection
  nulls: SortNulls | null
}

export interface SortModel {
  clauses: SortClause[]
  allowedFields: string[] | null
  isValid: boolean
  violations: string[]
  rawInput: string | null
  isEmpty: boolean
}

export const emptySortModel: SortModel = {
  clauses: [],
  allowedFields: null,
  isValid: true,
  violations: [],
  rawInput: null,
  isEmpty: true,
}
