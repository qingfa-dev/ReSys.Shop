import { describe, it, expect } from 'vitest'
import { toCountryQueryParams, COUNTRY_FILTER_FIELDS, COUNTRY_SORT_FIELDS } from '../country'
import { toStateQueryParams, STATE_FILTER_FIELDS, STATE_SORT_FIELDS } from '../state'

describe('toCountryQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toCountryQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter DSL for name (contains operator)', () => {
    const result = toCountryQueryParams({ name: 'United' })
    expect(result.filter).toBe('name*=United')
  })

  it('builds filter DSL for isoCode (equals operator)', () => {
    const result = toCountryQueryParams({ isoCode: 'US' })
    expect(result.filter).toBe('isoCode=US')
  })

  it('builds filter DSL for boolean isActive=true', () => {
    const result = toCountryQueryParams({ isActive: true })
    expect(result.filter).toBe('isActive=true')
  })

  it('builds filter DSL for boolean isActive=false', () => {
    const result = toCountryQueryParams({ isActive: false })
    expect(result.filter).toBe('isActive=false')
  })

  it('combines multiple filter conditions with comma', () => {
    const result = toCountryQueryParams({ name: 'Viet', isActive: true })
    expect(result.filter).toBe('name*=Viet,isActive=true')
  })

  it('builds sort ascending', () => {
    const result = toCountryQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toCountryQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('includes search and pagination', () => {
    const result = toCountryQueryParams({ search: 'California', page: 2, pageSize: 10 })
    expect(result.search).toBe('California')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(10)
  })

  it('skips empty string values in filters', () => {
    const result = toCountryQueryParams({ name: '', isoCode: 'US' })
    expect(result.filter).toBe('isoCode=US')
  })
})

describe('toStateQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toStateQueryParams({})
    expect(result.filter).toBeNull()
  })

  it('builds filter for countryId', () => {
    const result = toStateQueryParams({ countryId: 'abc-123' })
    expect(result.filter).toBe('countryId=abc-123')
  })

  it('builds filter for abbreviation', () => {
    const result = toStateQueryParams({ abbreviation: 'CA' })
    expect(result.filter).toBe('abbreviation=CA')
  })

  it('builds sort', () => {
    const result = toStateQueryParams({ sortBy: 'countryName', sortDirection: 'asc' })
    expect(result.sort).toEqual(['countryName'])
  })
})

describe('COUNTRY_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(COUNTRY_FILTER_FIELDS).toContain('name')
    expect(COUNTRY_FILTER_FIELDS).toContain('isoCode')
    expect(COUNTRY_FILTER_FIELDS).toContain('isActive')
  })
})

describe('COUNTRY_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(COUNTRY_SORT_FIELDS).toContain('name')
    expect(COUNTRY_SORT_FIELDS).toContain('createdAtUtc')
    expect(COUNTRY_SORT_FIELDS).toContain('modifiedAtUtc')
  })
})

describe('STATE_FILTER_FIELDS', () => {
  it('contains countryId for filtering by parent', () => {
    expect(STATE_FILTER_FIELDS).toContain('countryId')
  })
})

describe('STATE_SORT_FIELDS', () => {
  it('contains countryId for sorting by parent', () => {
    expect(STATE_SORT_FIELDS).toContain('countryId')
  })
})
