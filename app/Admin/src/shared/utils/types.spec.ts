import { describe, it, expect } from 'vitest'
import type { Nullable, Optional, Dictionary, Identifiable, Timestamped } from './types'

describe('types', () => {
  it('exports Nullable type', () => {
    const val: Nullable<string> = null
    expect(val).toBeNull()
  })

  it('exports Optional type', () => {
    const val: Optional<number> = undefined
    expect(val).toBeUndefined()
  })

  it('exports Dictionary type', () => {
    const dict: Dictionary<string> = { key: 'value' }
    expect(dict.key).toBe('value')
  })

  it('exports Identifiable type', () => {
    const obj: Identifiable = { id: 'abc' }
    expect(obj.id).toBe('abc')
  })

  it('exports Timestamped type', () => {
    const obj: Timestamped = { createdAt: '2025-01-01', updatedAt: '2025-06-01' }
    expect(obj.createdAt).toBe('2025-01-01')
  })
})
