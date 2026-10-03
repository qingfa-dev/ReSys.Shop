import { describe, it, expect } from 'vitest'
import { emptySortModel } from '../sort'

describe('emptySortModel', () => {
  it('is empty and valid', () => {
    expect(emptySortModel.isEmpty).toBe(true)
    expect(emptySortModel.isValid).toBe(true)
    expect(emptySortModel.clauses).toHaveLength(0)
  })
})
