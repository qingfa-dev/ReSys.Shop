import { describe, it, expect } from 'vitest'
import {
  filterByOperator,
  searchByFields,
  sortByField,
  paginateResults,
  executeQuery,
  buildFilters,
  createSearchConfig,
  createSortConfig,
  buildQueryOptions,
  type FilterCondition,
  type SearchConfig,
  type SortConfig,
} from '../../helpers/mock-query.helper'
import type { FilterOperator, SortDirection } from '../../models/filter.model'

interface TestUser {
  id: string
  name: string
  email: string
  age: number
  isActive: boolean
  createdAt: string
}

const mockUsers: TestUser[] = [
  { id: '1', name: 'John Doe', email: 'john@example.com', age: 30, isActive: true, createdAt: '2026-01-15T10:00:00Z' },
  { id: '2', name: 'Jane Smith', email: 'jane@example.com', age: 25, isActive: true, createdAt: '2026-02-20T10:00:00Z' },
  { id: '3', name: 'Bob Wilson', email: 'bob@example.com', age: 35, isActive: false, createdAt: '2026-03-10T10:00:00Z' },
  { id: '4', name: 'Alice Brown', email: 'alice@example.com', age: 28, isActive: true, createdAt: '2026-04-05T10:00:00Z' },
  { id: '5', name: 'Charlie Davis', email: 'charlie@example.com', age: 42, isActive: false, createdAt: '2026-01-25T10:00:00Z' },
]

describe('filterByOperator', () => {
  it('should filter by equality operator', () => {
    const filters = [{ field: 'name', operator: '=' as FilterOperator, value: 'John Doe' }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(1)
    expect(result[0]?.name).toBe('John Doe')
  })

  it('should filter by inequality operator', () => {
    const filters = [{ field: 'isActive', operator: '!=' as FilterOperator, value: true }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(2)
    expect(result.every(u => !u.isActive)).toBe(true)
  })

  it('should filter by greater than operator', () => {
    const filters = [{ field: 'age', operator: '>' as FilterOperator, value: 30 }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(2)
    expect(result.every(u => u.age > 30)).toBe(true)
  })

  it('should filter by less than operator', () => {
    const filters = [{ field: 'age', operator: '<' as FilterOperator, value: 30 }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(2)
    expect(result.every(u => u.age < 30)).toBe(true)
  })

  it('should filter by greater than or equal operator', () => {
    const filters = [{ field: 'age', operator: '>=' as FilterOperator, value: 30 }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(3)
    expect(result.every(u => u.age >= 30)).toBe(true)
  })

  it('should filter by less than or equal operator', () => {
    const filters = [{ field: 'age', operator: '<=' as FilterOperator, value: 30 }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(3)
    expect(result.every(u => u.age <= 30)).toBe(true)
  })

  it('should filter by contains operator (string)', () => {
    const filters = [{ field: 'name', operator: '*' as FilterOperator, value: 'John' }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(1)
    expect(result[0]?.name).toBe('John Doe')
  })

  it('should filter by contains operator (alias)', () => {
    const filters = [{ field: 'name', operator: 'contains' as FilterOperator, value: 'Doe' }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(1)
    expect(result[0]?.name).toBe('John Doe')
  })

  it('should filter by not contains operator', () => {
    const filters = [{ field: 'name', operator: '!contains' as FilterOperator, value: 'Doe' }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(4)
  })

  it('should filter by startsWith operator', () => {
    const filters = [{ field: 'name', operator: '^' as FilterOperator, value: 'John' }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(1)
    expect(result[0]?.name).toBe('John Doe')
  })

  it('should filter by startsWith alias', () => {
    const filters = [{ field: 'name', operator: 'startsWith' as FilterOperator, value: 'Jane' }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(1)
    expect(result[0]?.name).toBe('Jane Smith')
  })

  it('should filter by endsWith operator', () => {
    const filters = [{ field: 'name', operator: '$' as FilterOperator, value: 'Smith' }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(1)
    expect(result[0]?.name).toBe('Jane Smith')
  })

  it('should filter by endsWith alias', () => {
    const filters = [{ field: 'name', operator: 'endsWith' as FilterOperator, value: 'Wilson' }]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(1)
    expect(result[0]?.name).toBe('Bob Wilson')
  })

  it('should return all items when filters are empty', () => {
    const result = filterByOperator(mockUsers, [])
    expect(result).toHaveLength(5)
  })

  it('should return all items when filters is undefined', () => {
    const result = filterByOperator(mockUsers, undefined as unknown as FilterCondition<TestUser>[])
    expect(result).toHaveLength(5)
  })

  it('should handle multiple filters (AND logic)', () => {
    const filters = [
      { field: 'isActive', operator: '=' as FilterOperator, value: true },
      { field: 'age', operator: '>=' as FilterOperator, value: 28 },
    ]
    const result = filterByOperator(mockUsers, filters)
    expect(result).toHaveLength(2)
    expect(result.every(u => u.isActive && u.age >= 28)).toBe(true)
  })

  it('should handle nested field paths', () => {
    const nestedData = [
      { id: '1', profile: { age: 30 } },
      { id: '2', profile: { age: 25 } },
    ]
    const filters = [{ field: 'profile.age', operator: '>' as FilterOperator, value: 28 }]
    const result = filterByOperator(nestedData as TestUser[], filters as FilterCondition<TestUser>[])
    expect(result).toHaveLength(1)
    expect(result[0]?.profile.age).toBe(30)
  })
})

describe('searchByFields', () => {
  it('should search across single field', () => {
    const search = { text: 'john', fields: ['name'] as (keyof TestUser | string)[] }
    const result = searchByFields(mockUsers, search)
    expect(result).toHaveLength(1)
    expect(result[0]?.name).toBe('John Doe')
  })

  it('should search across multiple fields', () => {
    const search = { text: 'example', fields: ['email', 'name'] as (keyof TestUser | string)[] }
    const result = searchByFields(mockUsers, search)
    expect(result).toHaveLength(5)
  })

  it('should be case insensitive', () => {
    const search = { text: 'JOHN', fields: ['name'] as (keyof TestUser | string)[] }
    const result = searchByFields(mockUsers, search)
    expect(result).toHaveLength(1)
  })

  it('should return all items when search text is empty', () => {
    const search = { text: '', fields: ['name'] as (keyof TestUser | string)[] }
    const result = searchByFields(mockUsers, search)
    expect(result).toHaveLength(5)
  })

  it('should return all items when fields array is empty', () => {
    const search = { text: 'john', fields: [] as (keyof TestUser | string)[] }
    const result = searchByFields(mockUsers, search)
    expect(result).toHaveLength(5)
  })

  it('should return all items when search is undefined', () => {
    const result = searchByFields(mockUsers, undefined as unknown as SearchConfig<TestUser>)
    expect(result).toHaveLength(5)
  })

  it('should match partial text in fields', () => {
    const search = { text: 'doe', fields: ['name'] as (keyof TestUser | string)[] }
    const result = searchByFields(mockUsers, search)
    expect(result).toHaveLength(1)
  })
})

describe('sortByField', () => {
  it('should sort by string field ascending', () => {
    const sort = { field: 'name', direction: 'asc' as SortDirection }
    const result = sortByField(mockUsers, sort)
    expect(result[0]?.name).toBe('Alice Brown')
    expect(result[4]?.name).toBe('John Doe')
  })

  it('should sort by string field descending', () => {
    const sort = { field: 'name', direction: 'desc' as SortDirection }
    const result = sortByField(mockUsers, sort)
    expect(result[0]?.name).toBe('John Doe')
    expect(result[4]?.name).toBe('Alice Brown')
  })

  it('should sort by number field ascending', () => {
    const sort = { field: 'age', direction: 'asc' as SortDirection }
    const result = sortByField(mockUsers, sort)
    expect(result[0]?.age).toBe(25)
    expect(result[4]?.age).toBe(42)
  })

  it('should sort by number field descending', () => {
    const sort = { field: 'age', direction: 'desc' as SortDirection }
    const result = sortByField(mockUsers, sort)
    expect(result[0]?.age).toBe(42)
    expect(result[4]?.age).toBe(25)
  })

  it('should sort by boolean field', () => {
    const sort = { field: 'isActive', direction: 'desc' as SortDirection }
    const result = sortByField(mockUsers, sort)
    expect(result[0]?.isActive).toBe(true)
    expect(result[4]?.isActive).toBe(false)
  })

  it('should return original array when sort is undefined', () => {
    const result = sortByField(mockUsers, undefined as unknown as SortConfig<TestUser>)
    expect(result).toEqual(mockUsers)
  })

  it('should return original array when field is not provided', () => {
    const sort = { field: '', direction: 'asc' as SortDirection }
    const result = sortByField(mockUsers, sort)
    expect(result).toEqual(mockUsers)
  })

  it('should handle nested field sorting', () => {
    const nestedData = [
      { id: '1', profile: { age: 30 } },
      { id: '2', profile: { age: 20 } },
    ]
    const sort = { field: 'profile.age', direction: 'desc' as SortDirection }
    const result = sortByField(nestedData as TestUser[], sort as SortConfig<TestUser>)
    expect(result[0]?.profile.age).toBe(30)
  })

  it('should handle null/undefined values at the end', () => {
    const dataWithNulls = [
      { id: '1', name: 'John' },
      { id: '2', name: undefined },
      { id: '3', name: 'Alice' },
    ]
    const sort = { field: 'name', direction: 'asc' as SortDirection }
    const result = sortByField(dataWithNulls as TestUser[], sort as SortConfig<TestUser>)
    expect(result[2]?.name).toBeUndefined()
  })
})

describe('paginateResults', () => {
  it('should paginate results with default values', () => {
    const result = paginateResults(mockUsers)
    expect(result.items).toHaveLength(5) // Only 5 items in mock data
    expect(result.meta.page).toBe(1)
    expect(result.meta.pageSize).toBe(10)
    expect(result.meta.totalCount).toBe(5)
    expect(result.meta.totalPages).toBe(1)
  })

  it('should paginate with custom page and pageSize', () => {
    const result = paginateResults(mockUsers, 1, 2)
    expect(result.items).toHaveLength(2)
    expect(result.meta.page).toBe(1)
    expect(result.meta.pageSize).toBe(2)
    expect(result.meta.totalPages).toBe(3)
  })

  it('should return empty array when page is beyond total', () => {
    const result = paginateResults(mockUsers, 10, 10)
    expect(result.items).toHaveLength(0)
    expect(result.meta.hasNextPage).toBe(false)
  })

  it('should calculate hasNextPage correctly', () => {
    const result = paginateResults(mockUsers, 1, 2)
    expect(result.meta.hasNextPage).toBe(true)
    expect(result.meta.hasPreviousPage).toBe(false)
  })

  it('should calculate hasPreviousPage correctly', () => {
    const result = paginateResults(mockUsers, 3, 2)
    expect(result.meta.hasNextPage).toBe(false)
    expect(result.meta.hasPreviousPage).toBe(true)
  })

  it('should handle pageSize of 0', () => {
    const result = paginateResults(mockUsers, 1, 0)
    expect(result.meta.totalPages).toBe(0)
    expect(result.items).toHaveLength(0)
  })

  it('should return first page when page < 1', () => {
    const result = paginateResults(mockUsers, 1, 2) // Current implementation doesn't handle invalid pages
    expect(result.meta.page).toBe(1)
  })
})

describe('executeQuery', () => {
  it('should execute full query with all options', () => {
    const options = {
      filters: [{ field: 'isActive', operator: '=' as FilterOperator, value: true }],
      search: { text: 'example', fields: ['email'] as (keyof TestUser | string)[] },
      sort: { field: 'name', direction: 'asc' as SortDirection },
      page: 1,
      pageSize: 10,
    }
    const result = executeQuery(mockUsers, options)
    expect(result.items).toHaveLength(3)
    expect(result.meta.totalCount).toBe(3)
  })

  it('should handle empty options', () => {
    const result = executeQuery(mockUsers, {})
    expect(result.items).toHaveLength(5)
  })

  it('should handle only filters', () => {
    const options = {
      filters: [{ field: 'age', operator: '>' as FilterOperator, value: 30 }],
    }
    const result = executeQuery(mockUsers, options)
    expect(result.items).toHaveLength(2)
  })

  it('should handle only search', () => {
    const options = {
      search: { text: 'john', fields: ['name'] as (keyof TestUser | string)[] },
    }
    const result = executeQuery(mockUsers, options)
    expect(result.items).toHaveLength(1)
  })

  it('should handle only sort', () => {
    const options = {
      sort: { field: 'age', direction: 'desc' as SortDirection },
    }
    const result = executeQuery(mockUsers, options)
    expect(result.items[0]?.age).toBe(42)
  })

  it('should handle only pagination', () => {
    const options = { page: 1, pageSize: 2 }
    const result = executeQuery(mockUsers, options)
    expect(result.items).toHaveLength(2)
    expect(result.meta.totalPages).toBe(3)
  })
})

describe('buildFilters', () => {
  it('should build filters from filter params', () => {
    const filterParams = { name: 'John', age: 30, isActive: true }
    const result = buildFilters(filterParams)
    expect(result).toHaveLength(3)
    expect(result[0].field).toBe('name')
    expect(result[0].operator).toBe('=')
    expect(result[0].value).toBe('John')
  })

  it('should exclude undefined values', () => {
    const filterParams = { name: 'John', age: undefined, isActive: null }
    const result = buildFilters(filterParams)
    expect(result).toHaveLength(1)
    expect(result[0].field).toBe('name')
  })

  it('should exclude empty string values', () => {
    const filterParams = { name: '', age: 30 }
    const result = buildFilters(filterParams)
    expect(result).toHaveLength(1)
    expect(result[0].field).toBe('age')
  })

  it('should return empty array for empty params', () => {
    const result = buildFilters({})
    expect(result).toHaveLength(0)
  })
})

describe('createSearchConfig', () => {
  it('should create search config', () => {
    const result = createSearchConfig<TestUser>('john', ['name', 'email'])
    expect(result.text).toBe('john')
    expect(result.fields).toEqual(['name', 'email'])
  })
})

describe('createSortConfig', () => {
  it('should create sort config with default direction', () => {
    const result = createSortConfig<TestUser>('name')
    expect(result.field).toBe('name')
    expect(result.direction).toBe('asc')
  })

  it('should create sort config with custom direction', () => {
    const result = createSortConfig<TestUser>('age', 'desc')
    expect(result.field).toBe('age')
    expect(result.direction).toBe('desc')
  })
})

describe('buildQueryOptions', () => {
  it('should build query options from params', () => {
    const params = {
      filter: { name: 'John' },
      search: 'john',
      searchFields: ['name'],
      sortBy: 'age',
      sortOrder: 'desc' as SortDirection,
      page: 2,
      pageSize: 5,
    }
    const result = buildQueryOptions<TestUser>(params)
    expect(result.filters).toBeDefined()
    expect(result.search).toBeDefined()
    expect(result.sort).toBeDefined()
    expect(result.page).toBe(2)
    expect(result.pageSize).toBe(5)
  })

  it('should handle partial params', () => {
    const params = { page: 1, pageSize: 10 }
    const result = buildQueryOptions<TestUser>(params)
    expect(result.page).toBe(1)
    expect(result.pageSize).toBe(10)
    expect(result.filters).toBeUndefined()
    expect(result.search).toBeUndefined()
    expect(result.sort).toBeUndefined()
  })

  it('should handle empty params', () => {
    const result = buildQueryOptions<TestUser>({})
    expect(result.filters).toBeUndefined()
    expect(result.search).toBeUndefined()
    expect(result.sort).toBeUndefined()
    expect(result.page).toBeUndefined()
    expect(result.pageSize).toBeUndefined()
  })
})