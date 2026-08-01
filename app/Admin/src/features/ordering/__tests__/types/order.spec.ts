import { describe, it, expect } from 'vitest'
import {
  toOrderQueryParams,
  ORDER_FILTER_FIELDS,
  ORDER_SORT_FIELDS,
  ORDER_SEARCH_FIELDS,
} from '../../types/order'

describe('toOrderQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toOrderQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for status', () => {
    const result = toOrderQueryParams({ status: 'Placed' })
    expect(result.filter).toBe('Status=Placed')
  })

  it('builds filter DSL for checkoutState', () => {
    const result = toOrderQueryParams({ checkoutState: 'Payment' })
    expect(result.filter).toBe('CheckoutState=Payment')
  })

  it('builds filter DSL for currency', () => {
    const result = toOrderQueryParams({ currency: 'USD' })
    expect(result.filter).toBe('Currency=USD')
  })

  it('skips empty string currency in filters', () => {
    const result = toOrderQueryParams({ currency: '' })
    expect(result.filter).toBeNull()
  })

  it('builds sort ascending', () => {
    const result = toOrderQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'asc' })
    expect(result.sort).toEqual(['createdAtUtc'])
  })

  it('builds sort descending', () => {
    const result = toOrderQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes search and pagination through', () => {
    const result = toOrderQueryParams({ search: 'abc', page: 2, pageSize: 50 })
    expect(result.search).toBe('abc')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('ORDER_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(ORDER_FILTER_FIELDS).toEqual([
      'Status',
      'CheckoutState',
      'Currency',
      'UserId',
      'StoreId',
      'IsDeleted',
    ])
  })
})

describe('ORDER_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(ORDER_SORT_FIELDS).toEqual([
      'Number',
      'Total',
      'CompletedAtUtc',
      'CreatedAtUtc',
      'Status',
    ])
  })
})

describe('ORDER_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(ORDER_SEARCH_FIELDS).toEqual(['Number', 'Email'])
  })
})
