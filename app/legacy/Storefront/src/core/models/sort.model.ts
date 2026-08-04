/**
 * Sort Models
 * Handles ordering and sorting of results.
 */

export type SortDirection = 'asc' | 'desc'

export interface SortParams {
  sortBy?: string
  sortOrder?: SortDirection
  orderBy?: string[]
}

export interface SortFieldConfig {
  source: string
  defaultOrder?: SortDirection
  mapping?: Record<string, string>
}

export type SortableFields<T extends object> = {
  [K in keyof T & (string | number)]: T[K] extends object
    ? `${K}` | `${K}.${SortableFields<T[K]>}`
    : `${K}`
}[keyof T & (string | number)]