import { describe, it, expect } from 'vitest'
import { STORAGE_KEYS } from './storage'

describe('STORAGE_KEYS', () => {
  it('exports storage key constants', () => {
    expect(STORAGE_KEYS).toBeDefined()
    expect(typeof STORAGE_KEYS.ACCESS_TOKEN).toBe('string')
    expect(typeof STORAGE_KEYS.REFRESH_TOKEN).toBe('string')
  })
})
