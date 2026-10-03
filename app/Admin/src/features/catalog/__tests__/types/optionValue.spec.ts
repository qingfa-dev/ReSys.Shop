import { describe, it, expect } from 'vitest'
import { toOptionValueQueryParams, OPTION_VALUE_FILTER_FIELDS, OPTION_VALUE_SORT_FIELDS } from '../../types/optionValue'

describe('toOptionValueQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toOptionValueQueryParams({})
    expect(result.filter).toBeNull()
  })

  it('builds filter for optionTypeId', () => {
    const result = toOptionValueQueryParams({ optionTypeId: 'abc-123' })
    expect(result.filter).toBe('optionTypeId=abc-123')
  })

  it('builds filter for name (contains)', () => {
    const result = toOptionValueQueryParams({ name: 'Red' })
    expect(result.filter).toBe('name*=Red')
  })

  it('combines optionTypeId and name', () => {
    const result = toOptionValueQueryParams({ optionTypeId: 'abc-123', name: 'Red' })
    expect(result.filter).toBe('optionTypeId=abc-123,name*=Red')
  })

  it('builds sort', () => {
    const result = toOptionValueQueryParams({ sortBy: 'position', sortDirection: 'asc' })
    expect(result.sort).toEqual(['position'])
  })

  it('skips empty string values', () => {
    const result = toOptionValueQueryParams({ optionTypeId: 'abc-123', name: '' })
    expect(result.filter).toBe('optionTypeId=abc-123')
  })
})

describe('OPTION_VALUE_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(OPTION_VALUE_FILTER_FIELDS).toEqual([
      'optionTypeId',
      'name',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('OPTION_VALUE_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(OPTION_VALUE_SORT_FIELDS).toEqual([
      'name',
      'presentation',
      'position',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})
