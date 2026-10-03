import { describe, it, expect } from 'vitest'
import { emptyFilterGroup, emptyFilterModel, flattenConditions } from './filter'

describe('emptyFilterGroup', () => {
  it('has And logic with empty arrays', () => {
    expect(emptyFilterGroup.logic).toBe('And')
    expect(emptyFilterGroup.conditions).toHaveLength(0)
    expect(emptyFilterGroup.groups).toHaveLength(0)
  })
})

describe('emptyFilterModel', () => {
  it('is empty and valid', () => {
    expect(emptyFilterModel.isEmpty).toBe(true)
    expect(emptyFilterModel.isValid).toBe(true)
    expect(emptyFilterModel.root.logic).toBe('And')
  })
})

describe('flattenConditions', () => {
  it('flattens a flat group', () => {
    const conditions = [{ field: 'name', operator: 'Equal' as const, value: 'bolt' }]
    const result = flattenConditions({ logic: 'And', conditions, groups: [] })
    expect(result).toHaveLength(1)
    expect(result[0]!.field).toBe('name')
  })

  it('flattens nested groups recursively', () => {
    const group = {
      logic: 'And' as const,
      conditions: [{ field: 'a', operator: 'Equal' as const, value: '1' }],
      groups: [
        {
          logic: 'Or' as const,
          conditions: [{ field: 'b', operator: 'Equal' as const, value: '2' }],
          groups: [],
        },
      ],
    }
    const result = flattenConditions(group)
    expect(result).toHaveLength(2)
    expect(result[0]!.field).toBe('a')
    expect(result[1]!.field).toBe('b')
  })
})
