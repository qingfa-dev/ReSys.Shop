import { describe, it, expect } from 'vitest'
import { createFilterGroup, emptyFilterGroup } from '../filtering.model'

describe('createFilterGroup', () => {
  it('returns default group when called with no arguments', () => {
    expect(createFilterGroup()).toEqual({ logic: 'and', conditions: [], groups: [] })
  })

  it('returns group with logic "or" when called with "or"', () => {
    expect(createFilterGroup('or')).toEqual({ logic: 'or', conditions: [], groups: [] })
  })

  it('returns group with provided conditions', () => {
    expect(createFilterGroup('and', [{ field: 'name', op: '=', value: 'test' }])).toEqual({
      logic: 'and',
      conditions: [{ field: 'name', op: '=', value: 'test' }],
      groups: [],
    })
  })

  it('returns group with nested groups', () => {
    const result = createFilterGroup('and', [], [{ logic: 'and', conditions: [], groups: [] }])
    expect(result.groups).toHaveLength(1)
  })
})

describe('emptyFilterGroup', () => {
  it('is frozen', () => {
    expect(Object.isFrozen(emptyFilterGroup)).toBe(true)
  })

  it('has default values', () => {
    expect(emptyFilterGroup).toEqual({ logic: 'and', conditions: [], groups: [] })
  })
})
