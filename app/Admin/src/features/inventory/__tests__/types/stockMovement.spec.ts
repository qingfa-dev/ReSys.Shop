import { describe, it, expect } from 'vitest'
import {
  toStockMovementQueryParams,
  STOCK_MOVEMENT_FILTER_FIELDS,
  STOCK_MOVEMENT_SORT_FIELDS,
  STOCK_MOVEMENT_SEARCH_FIELDS,
} from '../../types/stockMovement'

describe('toStockMovementQueryParams', () => {
  it('returns null filter/search/sort and dedicated fields when query is empty', () => {
    const result = toStockMovementQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
    expect(result.fromUtc).toBeNull()
    expect(result.toUtc).toBeNull()
    expect(result.variantId).toBeNull()
    expect(result.stockLocationId).toBeNull()
  })

  it('passes dedicated date and reference fields through', () => {
    const result = toStockMovementQueryParams({
      fromUtc: '2026-01-01',
      toUtc: '2026-01-31',
      variantId: 'v-1',
      stockLocationId: 'l-1',
    })
    expect(result.fromUtc).toBe('2026-01-01')
    expect(result.toUtc).toBe('2026-01-31')
    expect(result.variantId).toBe('v-1')
    expect(result.stockLocationId).toBe('l-1')
    expect(result.filter).toBeNull()
  })

  it('builds sort ascending', () => {
    const result = toStockMovementQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'asc' })
    expect(result.sort).toEqual(['createdAtUtc'])
  })

  it('builds sort descending', () => {
    const result = toStockMovementQueryParams({ sortBy: 'quantity', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-quantity'])
  })

  it('passes pagination', () => {
    const result = toStockMovementQueryParams({ page: 2, pageSize: 50 })
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('STOCK_MOVEMENT_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_MOVEMENT_FILTER_FIELDS).toEqual([
      'StockItemId',
      'OriginatorType',
    ])
  })
})

describe('STOCK_MOVEMENT_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_MOVEMENT_SORT_FIELDS).toEqual([
      'Quantity',
      'CreatedAtUtc',
    ])
  })
})

describe('STOCK_MOVEMENT_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_MOVEMENT_SEARCH_FIELDS).toEqual(['Reason'])
  })
})
