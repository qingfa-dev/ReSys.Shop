/**
 * Mock Query Helper
 * Provides filter, search, sort, and pagination utilities for mock data.
 * Mirrors the API query parameter structure for seamless switching between mock and real data.
 */

import type { FilterOperator } from '../models/filter.model'
import type { SortDirection } from '../models/sort.model'
import type { PageMeta } from '../models/paging.model'

export type FilterCondition<T> = {
  field: keyof T | string
  operator: FilterOperator
  value: unknown
}

export type SearchConfig<T> = {
  text: string
  fields: (keyof T | string)[]
}

export type SortConfig<T> = {
  field: keyof T | string
  direction: SortDirection
}

export type QueryOptions<T> = {
  filters?: FilterCondition<T>[]
  search?: SearchConfig<T>
  sort?: SortConfig<T>
  page?: number
  pageSize?: number
}

function getNestedValue<T extends object>(obj: T, path: string): unknown {
  return path.split('.').reduce((acc: unknown, part: string) => {
    if (acc && typeof acc === 'object') {
      return (acc as Record<string, unknown>)[part]
    }
    return undefined
  }, obj)
}

export function filterByOperator<T extends object>(
  items: T[],
  filters: FilterCondition<T>[]
): T[] {
  if (!filters || filters.length === 0) return items

  return items.filter(item => {
    return filters.every(filter => {
      const value = getNestedValue(item, filter.field as string)
      const filterValue = filter.value

      switch (filter.operator) {
        case '=':
          return value === filterValue
        case '!=':
          return value !== filterValue
        case '>':
          return typeof value === 'number' && typeof filterValue === 'number' && value > filterValue
        case '<':
          return typeof value === 'number' && typeof filterValue === 'number' && value < filterValue
        case '>=':
          return typeof value === 'number' && typeof filterValue === 'number' && value >= filterValue
        case '<=':
          return typeof value === 'number' && typeof filterValue === 'number' && value <= filterValue
        case '*':
        case 'contains':
          if (typeof value === 'string' && typeof filterValue === 'string') {
            return value.toLowerCase().includes(filterValue.toLowerCase())
          }
          return false
        case '!*':
        case '!contains':
          if (typeof value === 'string' && typeof filterValue === 'string') {
            return !value.toLowerCase().includes(filterValue.toLowerCase())
          }
          return true
        case '^':
        case 'startsWith':
          if (typeof value === 'string' && typeof filterValue === 'string') {
            return value.toLowerCase().startsWith(filterValue.toLowerCase())
          }
          return false
        case '$':
        case 'endsWith':
          if (typeof value === 'string' && typeof filterValue === 'string') {
            return value.toLowerCase().endsWith(filterValue.toLowerCase())
          }
          return false
        default:
          return true
      }
    })
  })
}

export function searchByFields<T extends object>(
  items: T[],
  search: SearchConfig<T>
): T[] {
  if (!search || !search.text || !search.fields || search.fields.length === 0) {
    return items
  }

  const searchText = search.text.toLowerCase()
  return items.filter(item => {
    return search.fields.some(field => {
      const value = getNestedValue(item, field as string)
      if (typeof value === 'string') {
        return value.toLowerCase().includes(searchText)
      }
      return false
    })
  })
}

export function sortByField<T extends object>(
  items: T[],
  sort: SortConfig<T>
): T[] {
  if (!sort || !sort.field) return items

  const sorted = [...items]
  const field = sort.field as string
  const direction = sort.direction === 'desc' ? -1 : 1

  sorted.sort((a, b) => {
    const aValue = getNestedValue(a, field)
    const bValue = getNestedValue(b, field)

    if (aValue === undefined || aValue === null) return 1
    if (bValue === undefined || bValue === null) return -1

    if (typeof aValue === 'string' && typeof bValue === 'string') {
      return direction * aValue.localeCompare(bValue)
    }

    if (typeof aValue === 'number' && typeof bValue === 'number') {
      return direction * (aValue - bValue)
    }

    if (typeof aValue === 'boolean' && typeof bValue === 'boolean') {
      return direction * (aValue === bValue ? 0 : aValue ? 1 : -1)
    }

    if (aValue instanceof Date && bValue instanceof Date) {
      return direction * (aValue.getTime() - bValue.getTime())
    }

    return 0
  })

  return sorted
}

export function paginateResults<T>(
  items: T[],
  page = 1,
  pageSize = 10
): { items: T[]; meta: PageMeta } {
  const totalCount = items.length
  const totalPages = pageSize > 0 ? Math.ceil(totalCount / pageSize) : 0
  const startIndex = (page - 1) * pageSize
  const endIndex = startIndex + pageSize

  const paginatedItems = items.slice(startIndex, endIndex)

  const meta: PageMeta = {
    page,
    pageSize,
    totalCount,
    totalPages,
    hasNextPage: page < totalPages,
    hasPreviousPage: page > 1,
  }

  return { items: paginatedItems, meta }
}

export function executeQuery<T extends object>(
  items: T[],
  options: QueryOptions<T>
): { items: T[]; meta: PageMeta } {
  let result = [...items]

  if (options.filters && options.filters.length > 0) {
    result = filterByOperator(result, options.filters)
  }

  if (options.search) {
    result = searchByFields(result, options.search)
  }

  if (options.sort) {
    result = sortByField(result, options.sort)
  }

  const page = options.page ?? 1
  const pageSize = options.pageSize ?? 10

  return paginateResults(result, page, pageSize)
}

export function buildFilters<T extends object>(
  filterParams: Record<string, unknown>
): FilterCondition<T>[] {
  const conditions: FilterCondition<T>[] = []

  for (const [key, value] of Object.entries(filterParams)) {
    if (value !== undefined && value !== null && value !== '') {
      conditions.push({
        field: key,
        operator: '=',
        value,
      } as FilterCondition<T>)
    }
  }

  return conditions
}

export function createSearchConfig<T extends object>(
  searchText: string,
  searchFields: string[]
): SearchConfig<T> {
  return {
    text: searchText,
    fields: searchFields as (keyof T | string)[],
  }
}

export function createSortConfig<T extends object>(
  sortBy: string,
  sortOrder: SortDirection = 'asc'
): SortConfig<T> {
  return {
    field: sortBy as keyof T | string,
    direction: sortOrder,
  }
}

export function buildQueryOptions<T extends object>(params: {
  filter?: Record<string, unknown>
  search?: string
  searchFields?: string[]
  sortBy?: string
  sortOrder?: SortDirection
  page?: number
  pageSize?: number
}): QueryOptions<T> {
  const options: QueryOptions<T> = {}

  if (params.filter) {
    options.filters = buildFilters(params.filter)
  }

  if (params.search && params.searchFields && params.searchFields.length > 0) {
    options.search = createSearchConfig(params.search, params.searchFields)
  }

  if (params.sortBy) {
    options.sort = createSortConfig(params.sortBy, params.sortOrder ?? 'asc')
  }

  if (params.page) options.page = params.page
  if (params.pageSize) options.pageSize = params.pageSize

  return options
}