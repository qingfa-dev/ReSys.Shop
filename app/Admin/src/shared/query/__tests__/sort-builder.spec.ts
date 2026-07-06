import { describe, it, expect } from 'vitest'
import { SortBuilder } from '../sort-builder'

describe('SortBuilder', () => {
  it('builds single ascending sort', () => {
    const result = new SortBuilder().orderBy('name').build()
    expect(result).toEqual(['name:asc'])
  })

  it('builds single descending sort', () => {
    const result = new SortBuilder().orderByDesc('name').build()
    expect(result).toEqual(['name:desc'])
  })

  it('builds multi-sort chain', () => {
    const result = new SortBuilder()
      .orderBy('name')
      .thenByDesc('createdAt')
      .build()
    expect(result).toEqual(['name:asc', 'createdAt:desc'])
  })

  it('builds full sort chain with multiple thenBy', () => {
    const result = new SortBuilder()
      .orderBy('priority')
      .thenBy('name')
      .thenByDesc('createdAt')
      .build()
    expect(result).toEqual(['priority:asc', 'name:asc', 'createdAt:desc'])
  })

  it('returns undefined for empty builder', () => {
    const result = new SortBuilder().build()
    expect(result).toBeUndefined()
  })
})
