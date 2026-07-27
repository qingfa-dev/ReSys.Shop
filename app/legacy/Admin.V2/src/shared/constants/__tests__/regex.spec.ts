import { describe, it, expect } from 'vitest'
import { REGEX } from '../regex'

describe('REGEX', () => {
  it('EMAIL matches valid emails', () => {
    expect(REGEX.EMAIL.test('user@example.com')).toBe(true)
    expect(REGEX.EMAIL.test('not-email')).toBe(false)
  })

  it('is a non-empty record', () => {
    expect(Object.keys(REGEX).length).toBeGreaterThan(0)
  })
})
