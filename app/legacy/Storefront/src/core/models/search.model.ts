/**
 * Search Models
 * Handles global text search across multiple fields.
 */

export interface SearchParams {
  search?: string
  searchFields?: string[]
}

export interface SearchFieldConfig {
  source: string
  apiFields: string[]
}

export type SearchableFields<T extends object> = {
  [K in keyof T & (string | number)]: T[K] extends object
    ? `${K}` | `${K}.${SearchableFields<T[K]>}`
    : `${K}`
}[keyof T & (string | number)]