import { describe, it, expect } from 'vitest'
import { emptyQueryingModel, parseAll } from './querying'
import { isSuccess, isFailure } from '../result'

describe('emptyQueryingModel', () => {
  it('has all empty sub-models', () => {
    expect(emptyQueryingModel.filter.isEmpty).toBe(true)
    expect(emptyQueryingModel.search.isEmpty).toBe(true)
    expect(emptyQueryingModel.sort.isEmpty).toBe(true)
    expect(emptyQueryingModel.page.isEmpty).toBe(true)
  })
})

describe('parseAll', () => {
  it('parses empty params successfully', () => {
    const result = parseAll({})
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.filter.isEmpty).toBe(true)
      expect(result.value.page.page).toBe(1)
      expect(result.value.page.pageSize).toBe(20)
    }
  })

  it('parses null params successfully', () => {
    const result = parseAll({ filter: null, pageNumber: null, pageSize: null })
    expect(isSuccess(result)).toBe(true)
  })

  it('parses valid filter DSL', () => {
    const result = parseAll({ filter: 'name=bolt' })
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.filter.root.conditions).toHaveLength(1)
      expect(result.value.filter.root.conditions[0]!.field).toBe('name')
    }
  })

  it('parses valid sort string', () => {
    const result = parseAll({ sort: ['name'] })
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.sort.clauses).toHaveLength(1)
    }
  })

  it('parses valid search text', () => {
    const result = parseAll({ search: 'bolt' })
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.search.term.value).toBe('bolt')
    }
  })

  it('parses valid page values', () => {
    const result = parseAll({ pageNumber: 3, pageSize: 50 })
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.page.page).toBe(3)
      expect(result.value.page.pageSize).toBe(50)
    }
  })

  it('clamps page size to max', () => {
    const result = parseAll({ pageSize: 999 })
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.page.pageSize).toBe(100)
    }
  })

  it('fails on invalid filter DSL', () => {
    const result = parseAll({ filter: '{bad json}' })
    expect(isFailure(result)).toBe(true)
    if (isFailure(result)) {
      expect(result.errors[0]!.code).toBe('Filter.String.InvalidSyntax')
    }
  })

  it('fails on invalid page number', () => {
    const result = parseAll({ pageNumber: -1 })
    expect(isFailure(result)).toBe(true)
    if (isFailure(result)) {
      expect(result.statusCode).toBe(422)
    }
  })

  it('fails on non-integer page size', () => {
    const result = parseAll({ pageSize: 1.5 })
    expect(isFailure(result)).toBe(true)
  })

  it('enforces allowed filter fields whitelist', () => {
    const result = parseAll({ filter: 'secret=value' }, ['name'])
    expect(isFailure(result)).toBe(true)
    if (isFailure(result)) {
      expect(result.errors[0]!.code).toBe('Filter.Field.Disallowed')
    }
  })

  it('enforces allowed sort fields whitelist', () => {
    const result = parseAll({ sort: ['-secret'] }, null, ['name'])
    expect(isFailure(result)).toBe(true)
    if (isFailure(result)) {
      expect(result.errors[0]!.code).toBe('Sorting.Field.Disallowed')
    }
  })
})
