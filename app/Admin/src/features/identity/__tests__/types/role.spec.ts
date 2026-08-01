import { describe, it, expect } from 'vitest'
import {
  toRoleQueryParams,
  ROLE_FILTER_FIELDS,
  ROLE_SORT_FIELDS,
  ROLE_SEARCH_FIELDS,
} from '../../types/role'

describe('toRoleQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toRoleQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for name (contains operator)', () => {
    const result = toRoleQueryParams({ name: 'Admin' })
    expect(result.filter).toBe('name*=Admin')
  })

  it('builds sort ascending', () => {
    const result = toRoleQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toRoleQueryParams({ sortBy: 'name', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-name'])
  })

  it('skips empty string values in filters', () => {
    const result = toRoleQueryParams({ name: '' })
    expect(result.filter).toBeNull()
  })

  it('passes search and pagination', () => {
    const result = toRoleQueryParams({ search: 'admin', page: 2, pageSize: 20 })
    expect(result.search).toBe('admin')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(20)
  })
})

describe('ROLE_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(ROLE_FILTER_FIELDS).toEqual([
      'IsSystem',
      'CreatedAtUtc',
      'ModifiedAtUtc',
    ])
  })
})

describe('ROLE_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(ROLE_SORT_FIELDS).toEqual([
      'Name',
      'IsSystem',
      'CreatedAtUtc',
      'ModifiedAtUtc',
    ])
  })
})

describe('ROLE_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(ROLE_SEARCH_FIELDS).toEqual(['Name', 'Description'])
  })
})
