import { describe, it, expect } from 'vitest'
import {
  toShippingMethodQueryParams,
  SHIPPING_METHOD_FILTER_FIELDS,
  SHIPPING_METHOD_SORT_FIELDS,
  SHIPPING_METHOD_SEARCH_FIELDS,
} from '../../types/shippingMethod'

describe('toShippingMethodQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toShippingMethodQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for availableToUsers', () => {
    const result = toShippingMethodQueryParams({ availableToUsers: true })
    expect(result.filter).toBe('availableToUsers=true')
  })

  it('builds filter DSL for calculatorType', () => {
    const result = toShippingMethodQueryParams({ calculatorType: 'FlatRate' })
    expect(result.filter).toBe('calculatorType=FlatRate')
  })

  it('skips empty string calculatorType in filters', () => {
    const result = toShippingMethodQueryParams({ calculatorType: '' })
    expect(result.filter).toBeNull()
  })

  it('builds sort ascending', () => {
    const result = toShippingMethodQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toShippingMethodQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes search and pagination through', () => {
    const result = toShippingMethodQueryParams({ search: 'abc', page: 2, pageSize: 50 })
    expect(result.search).toBe('abc')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('SHIPPING_METHOD_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(SHIPPING_METHOD_FILTER_FIELDS).toEqual([
      'availableToUsers',
      'calculatorType',
      'taxCategoryId',
      'isDeleted',
    ])
  })
})

describe('SHIPPING_METHOD_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(SHIPPING_METHOD_SORT_FIELDS).toEqual([
      'name',
      'code',
      'position',
      'createdAtUtc',
    ])
  })
})

describe('SHIPPING_METHOD_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(SHIPPING_METHOD_SEARCH_FIELDS).toEqual(['name', 'code', 'adminName'])
  })
})
