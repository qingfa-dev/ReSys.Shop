import { describe, it, expect } from 'vitest'
import {
  toVariantQueryParams,
  VARIANT_FILTER_FIELDS,
  VARIANT_SORT_FIELDS,
  VARIANT_SEARCH_FIELDS,
} from '../../types/variant'

describe('toVariantQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toVariantQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter for isMaster true', () => {
    const result = toVariantQueryParams({ isMaster: true })
    expect(result.filter).toBe('isMaster=true')
  })

  it('omits filter when isMaster is false', () => {
    const result = toVariantQueryParams({ isMaster: false })
    expect(result.filter).toBeNull()
  })

  it('builds sort ascending', () => {
    const result = toVariantQueryParams({ sortBy: 'position', sortDirection: 'asc' })
    expect(result.sort).toEqual(['position'])
  })

  it('builds sort descending', () => {
    const result = toVariantQueryParams({ sortBy: 'sku', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-sku'])
  })

  it('passes search and pagination', () => {
    const result = toVariantQueryParams({ search: 'ABC', page: 2, pageSize: 25 })
    expect(result.search).toBe('ABC')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(25)
  })
})

describe('VARIANT_FILTER_FIELDS', () => {
  it('matches backend allowed filter fields', () => {
    expect(VARIANT_FILTER_FIELDS).toEqual([
      'isMaster',
      'trackInventory',
      'discontinuedOn',
      'dimensionsUnit',
      'weightUnit',
    ])
  })
})

describe('VARIANT_SORT_FIELDS', () => {
  it('matches backend allowed sort fields', () => {
    expect(VARIANT_SORT_FIELDS).toEqual([
      'sku',
      'position',
      'price',
      'weight',
      'height',
      'width',
      'depth',
    ])
  })
})

describe('VARIANT_SEARCH_FIELDS', () => {
  it('matches backend allowed search fields', () => {
    expect(VARIANT_SEARCH_FIELDS).toEqual(['sku', 'barcode', 'hsCode'])
  })
})
