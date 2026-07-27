import { describe, it, expect } from 'vitest'
import {
  flatAnd,
  flatOr,
  toStructuralKey,
  toDslString,
  conditionsFor,
  hasField,
  resolveSortClauses,
  hasSortField,
  clauseFor,
  resolveSearchFields,
  hasSearchField,
  totalPages,
  hasNextPage,
  hasPreviousPage,
  normalizePage,
  normalizePageSize,
} from './behaviors'
import { emptyFilterModel, emptyFilterGroup } from './filter'
import { emptySortModel } from './sort'
import { emptySearchModel } from './search'
import { emptyPageModel, defaultPageBounds } from './page'

describe('flatAnd / flatOr', () => {
  it('creates a filter group with given logic', () => {
    const conds = [{ field: 'name', operator: 'Equal' as const, value: 'bolt' }]
    const andGroup = flatAnd(conds)
    expect(andGroup.logic).toBe('And')
    expect(andGroup.conditions).toHaveLength(1)
    const orGroup = flatOr(conds)
    expect(orGroup.logic).toBe('Or')
  })
})

describe('toStructuralKey', () => {
  it('produces a deterministic key', () => {
    const group = { logic: 'And' as const, conditions: [{ field: 'a', operator: 'Equal' as const, value: '1' }], groups: [] }
    const key = toStructuralKey(group)
    expect(key).toContain('a:Equal:1')
    expect(key).toContain('[And]')
  })
})

describe('toDslString', () => {
  it('serializes a flat model', () => {
    const conditions = [{ field: 'name', operator: 'Equal' as const, value: 'bolt' }]
    const model = { ...emptyFilterModel, root: { logic: 'And' as const, conditions, groups: [] }, isEmpty: false }
    expect(toDslString(model)).toBe('name=bolt')
  })

  it('quotes values with commas', () => {
    const conditions = [{ field: 'name', operator: 'Equal' as const, value: 'foo,bar' }]
    const model = { ...emptyFilterModel, root: { logic: 'And' as const, conditions, groups: [] }, isEmpty: false }
    expect(toDslString(model)).toBe('name="foo,bar"')
  })
})

describe('conditionsFor / hasField', () => {
  const conditions = [
    { field: 'name', operator: 'Equal' as const, value: 'bolt' },
    { field: 'age', operator: 'GreaterThan' as const, value: '18' },
  ]
  const model = { ...emptyFilterModel, root: { logic: 'And' as const, conditions, groups: [] }, isEmpty: false }

  it('finds conditions by field', () => {
    expect(conditionsFor(model, 'name')).toHaveLength(1)
    expect(conditionsFor(model, 'missing')).toHaveLength(0)
  })

  it('checks field existence', () => {
    expect(hasField(model, 'name')).toBe(true)
    expect(hasField(model, 'missing')).toBe(false)
  })
})

describe('resolveSortClauses / hasSortField / clauseFor', () => {
  const clauses = [{ field: 'name', direction: 'Ascending' as const, nulls: null }]
  const model = { ...emptySortModel, clauses, isEmpty: false }

  it('returns model clauses when present', () => {
    expect(resolveSortClauses(model, [])).toHaveLength(1)
  })

  it('returns defaults when empty', () => {
    const defaults = [{ field: 'id', direction: 'Ascending' as const, nulls: null }]
    expect(resolveSortClauses(emptySortModel, defaults)).toHaveLength(1)
  })

  it('checks field existence', () => {
    expect(hasSortField(model, 'name')).toBe(true)
    expect(hasSortField(model, 'missing')).toBe(false)
  })

  it('finds clause by field', () => {
    expect(clauseFor(model, 'name')?.direction).toBe('Ascending')
    expect(clauseFor(model, 'missing')).toBeUndefined()
  })
})

describe('resolveSearchFields / hasSearchField', () => {
  const model = { ...emptySearchModel, fields: ['name', 'description'], isEmpty: false }

  it('returns model fields when present', () => {
    expect(resolveSearchFields(model, [])).toEqual(['name', 'description'])
  })

  it('returns defaults when empty', () => {
    expect(resolveSearchFields(emptySearchModel, ['id'])).toEqual(['id'])
  })

  it('checks field existence', () => {
    expect(hasSearchField(model, 'name')).toBe(true)
    expect(hasSearchField(model, 'missing')).toBe(false)
  })
})

describe('totalPages / hasNextPage / hasPreviousPage', () => {
  const model = { ...emptyPageModel, page: 2, pageSize: 10, isEmpty: false }

  it('calculates total pages', () => {
    expect(totalPages(model, 25)).toBe(3)
  })

  it('returns 0 for pageSize 0', () => {
    expect(totalPages({ ...model, pageSize: 0 }, 25)).toBe(0)
  })

  it('detects next page', () => {
    expect(hasNextPage(model, 25)).toBe(true)
    expect(hasNextPage(model, 5)).toBe(false)
  })

  it('detects previous page', () => {
    expect(hasPreviousPage(model)).toBe(true)
    expect(hasPreviousPage(emptyPageModel)).toBe(false)
  })
})

describe('normalizePage / normalizePageSize', () => {
  it('normalizes page', () => {
    expect(normalizePage(null)).toBe(1)
    expect(normalizePage(3)).toBe(3)
    expect(normalizePage(-1)).toBe(1)
    expect(normalizePage(1.5)).toBe(1)
  })

  it('normalizes page size', () => {
    expect(normalizePageSize(null)).toBe(20)
    expect(normalizePageSize(50)).toBe(50)
    expect(normalizePageSize(999, { defaultPage: 1, defaultPageSize: 20, maxPageSize: 100 })).toBe(100)
    expect(normalizePageSize(-1)).toBe(20)
  })
})
