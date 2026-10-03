import { describe, it, expect } from 'vitest'
import {
  toStockReservationQueryParams,
  STOCK_RESERVATION_FILTER_FIELDS,
  STOCK_RESERVATION_SORT_FIELDS,
  STOCK_RESERVATION_SEARCH_FIELDS,
} from '../../types/stockReservation'

describe('toStockReservationQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toStockReservationQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for variantId, orderId and state', () => {
    const result = toStockReservationQueryParams({ variantId: 'v-1', orderId: 'o-1', state: 'Reserved' })
    expect(result.filter).toBe('variantId=v-1,orderId=o-1,state=Reserved')
  })

  it('skips empty string values in filters', () => {
    const result = toStockReservationQueryParams({ variantId: '' })
    expect(result.filter).toBeNull()
  })

  it('builds sort ascending', () => {
    const result = toStockReservationQueryParams({ sortBy: 'expiresAtUtc', sortDirection: 'asc' })
    expect(result.sort).toEqual(['expiresAtUtc'])
  })

  it('builds sort descending', () => {
    const result = toStockReservationQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes pagination', () => {
    const result = toStockReservationQueryParams({ page: 2, pageSize: 50 })
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('STOCK_RESERVATION_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_RESERVATION_FILTER_FIELDS).toEqual([
      'variantId',
      'orderId',
      'state',
    ])
  })
})

describe('STOCK_RESERVATION_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_RESERVATION_SORT_FIELDS).toEqual([
      'expiresAtUtc',
      'createdAtUtc',
    ])
  })
})

describe('STOCK_RESERVATION_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_RESERVATION_SEARCH_FIELDS).toEqual([])
  })
})
