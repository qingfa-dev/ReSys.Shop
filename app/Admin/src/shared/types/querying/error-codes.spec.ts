import { describe, it, expect } from 'vitest'
import { FilterErrors, SortErrors, SearchErrors, PageErrors, filterError, sortError, searchError, pageError } from './error-codes'

describe('factory helpers', () => {
  it('filterError builds correct error shape', () => {
    const err = filterError('Test', 'msg')
    expect(err.code).toBe('Filter.Test')
    expect(err.message).toBe('msg')
    expect(err.type).toBe(422)
  })

  it('sortError builds correct error shape', () => {
    const err = sortError('Test', 'msg')
    expect(err.code).toBe('Sorting.Test')
  })

  it('searchError builds correct error shape', () => {
    const err = searchError('Test', 'msg')
    expect(err.code).toBe('Search.Test')
  })

  it('pageError builds correct error shape', () => {
    const err = pageError('Test', 'msg')
    expect(err.code).toBe('Paging.Test')
  })
})

describe('FilterErrors', () => {
  it('invalidSyntax', () => {
    const err = FilterErrors.invalidSyntax('foo')
    expect(err.code).toBe('Filter.String.InvalidSyntax')
    expect(err.message).toContain('foo')
  })

  it('invalidJson', () => {
    const err = FilterErrors.invalidJson('bad')
    expect(err.code).toBe('Filter.Json.InvalidStructure')
  })

  it('unknownOperator', () => {
    const err = FilterErrors.unknownOperator('??')
    expect(err.code).toBe('Filter.Operator.Unknown')
  })

  it('missingField', () => {
    const err = FilterErrors.missingField()
    expect(err.code).toBe('Filter.Field.Missing')
  })

  it('disallowedField', () => {
    const err = FilterErrors.disallowedField('secret')
    expect(err.code).toBe('Filter.Field.Disallowed')
    expect(err.message).toContain('secret')
  })

  it('missingOperator', () => {
    const err = FilterErrors.missingOperator()
    expect(err.code).toBe('Filter.Operator.Missing')
  })

  it('invalidTriplet', () => {
    const err = FilterErrors.invalidTriplet('a:b')
    expect(err.code).toBe('Filter.QueryString.InvalidTriplet')
  })
})

describe('SortErrors', () => {
  it('invalidSyntax', () => {
    const err = SortErrors.invalidSyntax('x')
    expect(err.code).toBe('Sorting.Parsing.InvalidSyntax')
  })

  it('invalidJson', () => {
    const err = SortErrors.invalidJson('bad')
    expect(err.code).toBe('Sorting.Parsing.InvalidJson')
  })

  it('disallowedField', () => {
    const err = SortErrors.disallowedField('x')
    expect(err.code).toBe('Sorting.Field.Disallowed')
  })

  it('unknownDirection', () => {
    const err = SortErrors.unknownDirection('up')
    expect(err.code).toBe('Sorting.Direction.Unknown')
  })

  it('unknownNulls', () => {
    const err = SortErrors.unknownNulls('maybe')
    expect(err.code).toBe('Sorting.Nulls.Unknown')
  })

  it('missingField', () => {
    const err = SortErrors.missingField()
    expect(err.code).toBe('Sorting.Field.Missing')
  })
})

describe('SearchErrors', () => {
  it('termRequired', () => {
    const err = SearchErrors.termRequired()
    expect(err.code).toBe('Search.Parsing.TermRequired')
  })

  it('invalidJson', () => {
    const err = SearchErrors.invalidJson('bad')
    expect(err.code).toBe('Search.Parsing.InvalidJson')
  })

  it('invalidQueryString', () => {
    const err = SearchErrors.invalidQueryString('bad')
    expect(err.code).toBe('Search.Parsing.InvalidQueryString')
  })
})

describe('PageErrors', () => {
  it('invalidJson', () => {
    const err = PageErrors.invalidJson('bad')
    expect(err.code).toBe('Paging.InvalidJson')
  })

  it('invalidNumber', () => {
    const err = PageErrors.invalidNumber('pageSize', 'abc')
    expect(err.code).toBe('Paging.InvalidNumber')
    expect(err.message).toContain('pageSize')
    expect(err.message).toContain('abc')
  })
})
