import { describe, it, expect } from 'vitest'
import { FilterOperator, FilterLogic, SearchMode, SortDirection, SortNulls } from './enums'

describe('FilterOperator', () => {
  it('defines 16 operator constants', () => {
    expect(Object.keys(FilterOperator)).toHaveLength(16)
    expect(FilterOperator.Equal).toBe('Equal')
    expect(FilterOperator.Contains).toBe('Contains')
    expect(FilterOperator.GreaterThan).toBe('GreaterThan')
  })
})

describe('FilterLogic', () => {
  it('defines And and Or', () => {
    expect(FilterLogic.And).toBe('And')
    expect(FilterLogic.Or).toBe('Or')
  })
})

describe('SearchMode', () => {
  it('defines Any and All', () => {
    expect(SearchMode.Any).toBe('Any')
    expect(SearchMode.All).toBe('All')
  })
})

describe('SortDirection', () => {
  it('defines Ascending and Descending', () => {
    expect(SortDirection.Ascending).toBe('Ascending')
    expect(SortDirection.Descending).toBe('Descending')
  })
})

describe('SortNulls', () => {
  it('defines First and Last', () => {
    expect(SortNulls.First).toBe('First')
    expect(SortNulls.Last).toBe('Last')
  })
})
