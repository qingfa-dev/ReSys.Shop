import { describe, it, expect } from 'vitest'
import {
  toStockTransferQueryParams,
  STOCK_TRANSFER_FILTER_FIELDS,
  STOCK_TRANSFER_SORT_FIELDS,
  STOCK_TRANSFER_SEARCH_FIELDS,
} from '../../types/stockTransfer'

describe('toStockTransferQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toStockTransferQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for state, sourceLocationId and destinationLocationId', () => {
    const result = toStockTransferQueryParams({
      state: 'InTransit',
      sourceLocationId: 'l-1',
      destinationLocationId: 'l-2',
    })
    expect(result.filter).toBe('state=InTransit,sourceLocationId=l-1,destinationLocationId=l-2')
  })

  it('skips empty string values in location filters', () => {
    const result = toStockTransferQueryParams({ sourceLocationId: '' })
    expect(result.filter).toBeNull()
  })

  it('builds sort ascending', () => {
    const result = toStockTransferQueryParams({ sortBy: 'number', sortDirection: 'asc' })
    expect(result.sort).toEqual(['number'])
  })

  it('builds sort descending', () => {
    const result = toStockTransferQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes pagination', () => {
    const result = toStockTransferQueryParams({ page: 2, pageSize: 50 })
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('STOCK_TRANSFER_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_TRANSFER_FILTER_FIELDS).toEqual([
      'state',
      'sourceLocationId',
      'destinationLocationId',
    ])
  })
})

describe('STOCK_TRANSFER_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_TRANSFER_SORT_FIELDS).toEqual([
      'number',
      'state',
      'createdAtUtc',
    ])
  })
})

describe('STOCK_TRANSFER_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_TRANSFER_SEARCH_FIELDS).toEqual([])
  })
})
