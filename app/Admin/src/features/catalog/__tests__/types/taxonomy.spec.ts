import { describe, it, expect } from 'vitest'
import { toTaxonomyQueryParams, TAXONOMY_FILTER_FIELDS, TAXONOMY_SORT_FIELDS } from '../../types/taxonomy'

describe('toTaxonomyQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toTaxonomyQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter DSL for name (contains operator)', () => {
    const result = toTaxonomyQueryParams({ name: 'Categories' })
    expect(result.filter).toBe('name*=Categories')
  })

  it('builds sort ascending', () => {
    const result = toTaxonomyQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toTaxonomyQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('skips empty string values in filters', () => {
    const result = toTaxonomyQueryParams({ name: '' })
    expect(result.filter).toBeNull()
  })

  it('passes search and pagination', () => {
    const result = toTaxonomyQueryParams({ search: 'test', page: 2, pageSize: 10 })
    expect(result.search).toBe('test')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(10)
  })
})

describe('TAXONOMY_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(TAXONOMY_FILTER_FIELDS).toEqual([
      'name',
      'taxonsCount',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('TAXONOMY_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(TAXONOMY_SORT_FIELDS).toEqual([
      'name',
      'presentation',
      'position',
      'taxonsCount',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})
