import { describe, it, expect } from 'vitest'
import {
  toProfileQueryParams,
  PROFILE_FILTER_FIELDS,
  PROFILE_SORT_FIELDS,
  PROFILE_SEARCH_FIELDS,
} from '../../types/profile'

describe('toProfileQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toProfileQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for gender', () => {
    const result = toProfileQueryParams({ gender: 'Male' })
    expect(result.filter).toBe('Gender=Male')
  })

  it('skips empty string gender in filters', () => {
    const result = toProfileQueryParams({ gender: '' })
    expect(result.filter).toBeNull()
  })

  it('builds filter DSL for isActive', () => {
    const result = toProfileQueryParams({ isActive: true })
    expect(result.filter).toBe('IsActive=true')
  })

  it('builds sort ascending', () => {
    const result = toProfileQueryParams({ sortBy: 'firstName', sortDirection: 'asc' })
    expect(result.sort).toEqual(['firstName'])
  })

  it('builds sort descending', () => {
    const result = toProfileQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes search and pagination through', () => {
    const result = toProfileQueryParams({ search: 'abc', page: 2, pageSize: 50 })
    expect(result.search).toBe('abc')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('PROFILE_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PROFILE_FILTER_FIELDS).toEqual([
      'Gender',
      'IsActive',
      'CreatedAtUtc',
      'ModifiedAtUtc',
    ])
  })
})

describe('PROFILE_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PROFILE_SORT_FIELDS).toEqual([
      'FirstName',
      'LastName',
      'CreatedAtUtc',
      'ModifiedAtUtc',
    ])
  })
})

describe('PROFILE_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PROFILE_SEARCH_FIELDS).toEqual(['FirstName', 'LastName', 'Email', 'Bio'])
  })
})
