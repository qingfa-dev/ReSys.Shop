import { describe, it, expect } from 'vitest'
import { toTaxonQueryParams, TAXON_FILTER_FIELDS, TAXON_SORT_FIELDS, TAXON_SORT_ORDERS, TAXON_MATCH_POLICIES } from '../../types/taxon'

describe('toTaxonQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toTaxonQueryParams({})
    expect(result.filter).toBeNull()
  })

  it('builds filter for taxonomyId', () => {
    const result = toTaxonQueryParams({ taxonomyId: 'abc-123' })
    expect(result.filter).toBe('taxonomyId=abc-123')
  })

  it('builds filter for name contains', () => {
    const result = toTaxonQueryParams({ name: 'Shoes' })
    expect(result.filter).toBe('name*=Shoes')
  })

  it('combines taxonomyId and name filters', () => {
    const result = toTaxonQueryParams({ taxonomyId: 'abc-123', name: 'Shoes' })
    expect(result.filter).toBe('taxonomyId=abc-123,name*=Shoes')
  })

  it('builds sort ascending', () => {
    const result = toTaxonQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('passes through a raw filter string', () => {
    const result = toTaxonQueryParams({ filter: 'depth=1' })
    expect(result.filter).toBe('depth=1')
  })

  it('forwards search fields and mode', () => {
    const result = toTaxonQueryParams({ search: 'shoes', searchFields: ['name', 'slug'], searchMode: 'any' })
    expect(result.search).toBe('shoes')
    expect(result.searchFields).toEqual(['name', 'slug'])
    expect(result.searchMode).toBe('any')
  })

  it('merges raw filter with taxonomyId filter', () => {
    const result = toTaxonQueryParams({ filter: 'depth=1', taxonomyId: 'abc-123' })
    expect(result.filter).toBe('depth=1,taxonomyId=abc-123')
  })
})

describe('TAXON_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(TAXON_FILTER_FIELDS).toEqual([
      'taxonomyId',
      'name',
      'slug',
      'depth',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('TAXON_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(TAXON_SORT_FIELDS).toEqual([
      'name',
      'slug',
      'position',
      'depth',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('TAXON_SORT_ORDERS', () => {
  it('contains all 8 sort orders', () => {
    expect(TAXON_SORT_ORDERS).toHaveLength(8)
    expect(TAXON_SORT_ORDERS[0]).toBe('Manual')
  })
})

describe('TAXON_MATCH_POLICIES', () => {
  it('contains All and Any', () => {
    expect(TAXON_MATCH_POLICIES).toEqual(['All', 'Any'])
  })
})
