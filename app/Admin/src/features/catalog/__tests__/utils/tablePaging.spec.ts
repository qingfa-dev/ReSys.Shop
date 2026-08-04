import { describe, it, expect } from 'vitest'
import { tableFirst } from '../../utils/tablePaging'

describe('tableFirst', () => {
  it('computes first row index', () => {
    expect(tableFirst(1, 25)).toBe(0)
    expect(tableFirst(2, 25)).toBe(25)
    expect(tableFirst(3, 10)).toBe(20)
  })
})
