import type { SearchMode } from './enums'

export interface SearchTerm {
  value: string
  caseSensitive: boolean
}

export const defaultSearchTerm: SearchTerm = {
  value: '',
  caseSensitive: false,
}

export interface SearchModel {
  term: SearchTerm
  fields: string[]
  mode: SearchMode
  allowedFields: string[] | null
  isValid: boolean
  violations: string[]
  rawInput: string | null
  isEmpty: boolean
}

export const emptySearchModel: SearchModel = {
  term: defaultSearchTerm,
  fields: [],
  mode: 'Any',
  allowedFields: null,
  isValid: true,
  violations: [],
  rawInput: null,
  isEmpty: true,
}
