import { describe, it, expect } from 'vitest'
import { toProductQueryParams, PRODUCT_FILTER_FIELDS, PRODUCT_SORT_FIELDS } from '../../types/product'

describe('toProductQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toProductQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter for status exact', () => {
    const result = toProductQueryParams({ status: 'Active' })
    expect(result.filter).toBe('status=Active')
  })

  it('builds filter for seasonName contains', () => {
    const result = toProductQueryParams({ season: 'Summer' })
    expect(result.filter).toBe('seasonName*=Summer')
  })

  it('builds filter for taxonId exact', () => {
    const result = toProductQueryParams({ taxonId: 'abc-123' })
    expect(result.filter).toBe('taxonId=abc-123')
  })

  it('combines multiple filters', () => {
    const result = toProductQueryParams({ status: 'Active', season: 'Winter' })
    expect(result.filter).toBe('status=Active,seasonName*=Winter')
  })

  it('builds sort ascending', () => {
    const result = toProductQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toProductQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes pagination', () => {
    const result = toProductQueryParams({ page: 3, pageSize: 15 })
    expect(result.pageNumber).toBe(3)
    expect(result.pageSize).toBe(15)
  })
})

describe('PRODUCT_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PRODUCT_FILTER_FIELDS).toEqual([
      'status',
      'seasonName',
      'department',
      'createdAtUtc',
      'availableOn',
    ])
  })
})

describe('PRODUCT_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PRODUCT_SORT_FIELDS).toEqual([
      'name',
      'createdAtUtc',
      'modifiedAtUtc',
      'availableOn',
      'variantsCount',
    ])
  })
})
