import { describe, it, expect } from 'vitest'
import {
  toUserQueryParams,
  USER_FILTER_FIELDS,
  USER_SORT_FIELDS,
  USER_SEARCH_FIELDS,
} from '../../types/user'

describe('toUserQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toUserQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for isActive', () => {
    const result = toUserQueryParams({ isActive: true })
    expect(result.filter).toBe('IsActive=true')
  })

  it('builds filter DSL for emailConfirmed false', () => {
    const result = toUserQueryParams({ emailConfirmed: false })
    expect(result.filter).toBe('EmailConfirmed=false')
  })

  it('builds filter DSL for phoneNumberConfirmed', () => {
    const result = toUserQueryParams({ phoneNumberConfirmed: true })
    expect(result.filter).toBe('PhoneNumberConfirmed=true')
  })

  it('builds sort ascending', () => {
    const result = toUserQueryParams({ sortBy: 'userName', sortDirection: 'asc' })
    expect(result.sort).toEqual(['userName'])
  })

  it('builds sort descending', () => {
    const result = toUserQueryParams({ sortBy: 'userName', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-userName'])
  })

  it('passes search and pagination', () => {
    const result = toUserQueryParams({ search: 'admin', page: 2, pageSize: 50 })
    expect(result.search).toBe('admin')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('USER_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(USER_FILTER_FIELDS).toEqual([
      'IsActive',
      'EmailConfirmed',
      'PhoneNumberConfirmed',
      'CreatedAtUtc',
      'ModifiedAtUtc',
    ])
  })
})

describe('USER_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(USER_SORT_FIELDS).toEqual([
      'UserName',
      'Email',
      'CreatedAtUtc',
      'ModifiedAtUtc',
      'LastLoginAtUtc',
    ])
  })
})

describe('USER_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(USER_SEARCH_FIELDS).toEqual(['UserName', 'Email', 'FirstName', 'LastName'])
  })
})
