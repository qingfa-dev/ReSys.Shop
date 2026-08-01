import { describe, it, expect } from 'vitest'
import {
  toPaymentMethodQueryParams,
  PAYMENT_METHOD_FILTER_FIELDS,
  PAYMENT_METHOD_SORT_FIELDS,
  PAYMENT_METHOD_SEARCH_FIELDS,
} from '../../types/paymentMethod'

describe('toPaymentMethodQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toPaymentMethodQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for active', () => {
    const result = toPaymentMethodQueryParams({ active: true })
    expect(result.filter).toBe('Active=true')
  })

  it('builds filter DSL for providerKey', () => {
    const result = toPaymentMethodQueryParams({ providerKey: 'stripe' })
    expect(result.filter).toBe('ProviderKey=stripe')
  })

  it('skips empty string providerKey in filters', () => {
    const result = toPaymentMethodQueryParams({ providerKey: '' })
    expect(result.filter).toBeNull()
  })

  it('builds filter DSL for autoCapture', () => {
    const result = toPaymentMethodQueryParams({ autoCapture: true })
    expect(result.filter).toBe('AutoCapture=true')
  })

  it('builds sort ascending', () => {
    const result = toPaymentMethodQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toPaymentMethodQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes search and pagination through', () => {
    const result = toPaymentMethodQueryParams({ search: 'abc', page: 2, pageSize: 50 })
    expect(result.search).toBe('abc')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('PAYMENT_METHOD_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PAYMENT_METHOD_FILTER_FIELDS).toEqual([
      'Active',
      'ProviderKey',
      'AutoCapture',
      'DisplayOn',
      'IsDeleted',
    ])
  })
})

describe('PAYMENT_METHOD_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PAYMENT_METHOD_SORT_FIELDS).toEqual([
      'Name',
      'Position',
      'CreatedAtUtc',
    ])
  })
})

describe('PAYMENT_METHOD_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PAYMENT_METHOD_SEARCH_FIELDS).toEqual(['Name', 'Code', 'Description'])
  })
})
