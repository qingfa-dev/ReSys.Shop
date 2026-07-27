import { describe, it, expect } from 'vitest'
import { PERMISSIONS } from '../permissions'

function collectLeafValues(obj: Record<string, unknown>): unknown[] {
  return Object.values(obj).flatMap(v =>
    typeof v === 'object' && v !== null
      ? collectLeafValues(v as Record<string, unknown>)
      : [v],
  )
}

describe('PERMISSIONS', () => {
  it('is a non-empty record of string constants', () => {
    expect(Object.keys(PERMISSIONS).length).toBeGreaterThan(0)
    const leaves = collectLeafValues(PERMISSIONS as any)
    expect(leaves.length).toBeGreaterThan(0)
    for (const val of leaves) {
      expect(typeof val).toBe('string')
      expect((val as string).length).toBeGreaterThan(0)
    }
  })
})
