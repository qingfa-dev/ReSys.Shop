import { describe, it, expect } from 'vitest'
import {
  toStockLocationQueryParams,
  STOCK_LOCATION_FILTER_FIELDS,
  STOCK_LOCATION_SORT_FIELDS,
  STOCK_LOCATION_SEARCH_FIELDS,
} from '../../types/stockLocation'

describe('toStockLocationQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toStockLocationQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for active, default and isDeleted', () => {
    const result = toStockLocationQueryParams({ active: true, default: true, isDeleted: true })
    expect(result.filter).toBe('Active=true,Default=true,IsDeleted=true')
  })

  it('skips active when false', () => {
    const result = toStockLocationQueryParams({ active: false })
    expect(result.filter).toBeNull()
  })

  it('builds sort ascending', () => {
    const result = toStockLocationQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toStockLocationQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes search and pagination', () => {
    const result = toStockLocationQueryParams({ search: 'main', page: 2, pageSize: 50 })
    expect(result.search).toBe('main')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('STOCK_LOCATION_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_LOCATION_FILTER_FIELDS).toEqual([
      'Active',
      'Default',
      'BackorderableDefault',
      'IsDeleted',
      'CountryId',
      'StateId',
    ])
  })
})

describe('STOCK_LOCATION_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_LOCATION_SORT_FIELDS).toEqual([
      'Name',
      'Code',
      'Position',
      'CreatedAtUtc',
    ])
  })
})

describe('STOCK_LOCATION_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_LOCATION_SEARCH_FIELDS).toEqual([
      'Name',
      'Code',
      'City',
      'AdminName',
    ])
  })
})
