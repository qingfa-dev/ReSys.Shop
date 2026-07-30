import { describe, it, expect } from 'vitest'
import { toOptionTypeQueryParams, OPTION_TYPE_FILTER_FIELDS, OPTION_TYPE_SORT_FIELDS } from '../../types/optionType'

describe('toOptionTypeQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toOptionTypeQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter DSL for name (contains operator)', () => {
    const result = toOptionTypeQueryParams({ name: 'Size' })
    expect(result.filter).toBe('name*=Size')
  })

  it('builds filter DSL for filterable=true', () => {
    const result = toOptionTypeQueryParams({ filterable: true })
    expect(result.filter).toBe('filterable=true')
  })

  it('builds filter DSL for filterable=false', () => {
    const result = toOptionTypeQueryParams({ filterable: false })
    expect(result.filter).toBe('filterable=false')
  })

  it('combines multiple filter conditions with comma', () => {
    const result = toOptionTypeQueryParams({ name: 'Color', filterable: true })
    expect(result.filter).toBe('name*=Color,filterable=true')
  })

  it('builds sort ascending', () => {
    const result = toOptionTypeQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toOptionTypeQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('skips empty string values in filters', () => {
    const result = toOptionTypeQueryParams({ name: '', filterable: true })
    expect(result.filter).toBe('filterable=true')
  })
})

describe('OPTION_TYPE_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(OPTION_TYPE_FILTER_FIELDS).toEqual([
      'name',
      'filterable',
      'optionValuesCount',
      'productsCount',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('OPTION_TYPE_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(OPTION_TYPE_SORT_FIELDS).toEqual([
      'name',
      'presentation',
      'position',
      'optionValuesCount',
      'productsCount',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})
