/**
 * Filter Models
 * Handles dynamic filtering with operators and DSL syntax.
 */

export type FilterOperator = '=' | '!=' | '>' | '<' | '>=' | '<=' | '!*' | '*' | '^' | '$' | 'contains' | '!contains' | 'startsWith' | 'endsWith'

export interface FilterParams {
  filter?: string
}

export type FilterMap = Record<string, unknown>

export type NestedKeyOf<T extends object> = {
  [K in keyof T & (string | number)]: T[K] extends object
  ? `${K}` | `${K}.${NestedKeyOf<T[K]>}`
  : `${K}`
}[keyof T & (string | number)]

export interface FieldMapping {
  source: string
  target: string
  operator?: FilterOperator
  transform?: (value: unknown) => unknown
  skip?: (value: unknown) => boolean
}

export type FilterSchema = Record<string, unknown>
