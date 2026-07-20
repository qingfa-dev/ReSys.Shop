import { describe, it, expect } from 'vitest'
import { toCamelCase } from '../string.transforms'

describe('toCamelCase', () => {
  it('converts snake_case to camelCase', () => {
    expect(toCamelCase('snake_case')).toBe('snakeCase')
  })

  it('converts multiple underscores', () => {
    expect(toCamelCase('foo_bar_baz')).toBe('fooBarBaz')
  })

  it('returns already camelCase as-is', () => {
    expect(toCamelCase('already')).toBe('already')
  })

  it('returns empty string as-is', () => {
    expect(toCamelCase('')).toBe('')
  })

  it('handles leading underscore', () => {
    expect(toCamelCase('_leading')).toBe('Leading')
  })
})
