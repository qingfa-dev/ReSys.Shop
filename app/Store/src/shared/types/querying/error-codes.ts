import type { ApiError } from '../error'
import { ErrorType } from '../error'

// Create: Typed API error with domain-prefixed code for grep-based auditing
export function filterError(code: string, message: string): ApiError {
  return { code: `Filter.${code}`, message, type: ErrorType.Validation }
}

export function sortError(code: string, message: string): ApiError {
  return { code: `Sorting.${code}`, message, type: ErrorType.Validation }
}

export function searchError(code: string, message: string): ApiError {
  return { code: `Search.${code}`, message, type: ErrorType.Validation }
}

export function pageError(code: string, message: string): ApiError {
  return { code: `Paging.${code}`, message, type: ErrorType.Validation }
}

// Enforce: Filter validation error catalog — used by parsers for consistent error messages
export const FilterErrors = {
  invalidSyntax: (raw: string) => filterError('String.InvalidSyntax', `Invalid filter syntax: "${raw}".`),
  invalidJson: (detail: string) => filterError('Json.InvalidStructure', `Invalid filter JSON: ${detail}.`),
  unknownOperator: (token: string) => filterError('Operator.Unknown', `Unknown filter operator: "${token}".`),
  missingField: () => filterError('Field.Missing', 'Filter condition must specify a field.'),
  disallowedField: (field: string) => filterError('Field.Disallowed', `Filter field "${field}" is not allowed.`),
  missingOperator: () => filterError('Operator.Missing', 'Filter condition must specify an operator.'),
  invalidTriplet: (entry: string) => filterError('QueryString.InvalidTriplet', `Invalid filter triplet: "${entry}".`),
} as const

// Enforce: Sort validation error catalog
export const SortErrors = {
  invalidSyntax: (raw: string) => sortError('Parsing.InvalidSyntax', `Invalid sort syntax: "${raw}".`),
  invalidJson: (detail: string) => sortError('Parsing.InvalidJson', `Invalid sort JSON: ${detail}.`),
  disallowedField: (field: string) => sortError('Field.Disallowed', `Sort field "${field}" is not allowed.`),
  unknownDirection: (value: string) => sortError('Direction.Unknown', `Unknown sort direction: "${value}".`),
  unknownNulls: (value: string) => sortError('Nulls.Unknown', `Unknown sort nulls: "${value}".`),
  missingField: () => sortError('Field.Missing', 'Sort clause must specify a field.'),
} as const

// Enforce: Search validation error catalog
export const SearchErrors = {
  termRequired: () => searchError('Parsing.TermRequired', 'Search term is required.'),
  invalidJson: (detail: string) => searchError('Parsing.InvalidJson', `Invalid search JSON: ${detail}.`),
  invalidQueryString: (detail: string) => searchError('Parsing.InvalidQueryString', `Invalid search query string: ${detail}.`),
} as const

// Enforce: Pagination validation error catalog
export const PageErrors = {
  invalidJson: (detail: string) => pageError('InvalidJson', `Invalid page JSON: ${detail}.`),
  invalidNumber: (property: string, value: string) => pageError('InvalidNumber', `Invalid ${property}: "${value}".`),
} as const
