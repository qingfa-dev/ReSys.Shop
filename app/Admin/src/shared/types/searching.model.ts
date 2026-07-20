export type SearchMode = 'any' | 'all'

export interface SearchTerm {
  value: string
  caseSensitive: boolean
}

export interface SearchModel {
  term: SearchTerm
  fields: string[]
  mode: SearchMode
  allowedFields?: string[]
  isValid: boolean
  violations: string[]
  isEmpty: boolean
}

export const emptySearchModel: SearchModel = Object.freeze({
  term: { value: '', caseSensitive: false },
  fields: [],
  mode: 'any',
  isValid: true,
  violations: [],
  isEmpty: true,
})
