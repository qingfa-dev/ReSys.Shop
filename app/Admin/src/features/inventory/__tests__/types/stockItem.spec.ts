import { describe, it, expect } from 'vitest'
import {
  toStockItemQueryParams,
  STOCK_ITEM_FILTER_FIELDS,
  STOCK_ITEM_SORT_FIELDS,
  STOCK_ITEM_SEARCH_FIELDS,
} from '../../types/stockItem'

describe('toStockItemQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toStockItemQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
    expect(result.pageNumber).toBeNull()
    expect(result.pageSize).toBeNull()
  })

  it('builds filter DSL for stockLocationId, variantId and backorderable', () => {
    const result = toStockItemQueryParams({ stockLocationId: 'l-1', variantId: 'v-1', backorderable: true })
    expect(result.filter).toBe('stockLocationId=l-1,variantId=v-1,backorderable=true')
  })

  it('skips backorderable when false', () => {
    const result = toStockItemQueryParams({ backorderable: false })
    expect(result.filter).toBeNull()
  })

  it('skips empty string values in filters', () => {
    const result = toStockItemQueryParams({ stockLocationId: '' })
    expect(result.filter).toBeNull()
  })

  it('builds sort ascending', () => {
    const result = toStockItemQueryParams({ sortBy: 'countOnHand', sortDirection: 'asc' })
    expect(result.sort).toEqual(['countOnHand'])
  })

  it('builds sort descending', () => {
    const result = toStockItemQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes pagination', () => {
    const result = toStockItemQueryParams({ page: 2, pageSize: 50 })
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(50)
  })
})

describe('STOCK_ITEM_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_ITEM_FILTER_FIELDS).toEqual([
      'stockLocationId',
      'variantId',
      'backorderable',
    ])
  })
})

describe('STOCK_ITEM_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_ITEM_SORT_FIELDS).toEqual([
      'countOnHand',
      'createdAtUtc',
    ])
  })
})

describe('STOCK_ITEM_SEARCH_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(STOCK_ITEM_SEARCH_FIELDS).toEqual([])
  })
})
