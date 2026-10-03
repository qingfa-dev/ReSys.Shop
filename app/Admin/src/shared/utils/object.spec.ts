import { describe, it, expect } from 'vitest'
import { toCamelCaseKeys } from './object'

describe('toCamelCaseKeys', () => {
  it('converts top-level keys', () => {
    const result = toCamelCaseKeys({ first_name: 'John', last_name: 'Doe' })
    expect(result).toEqual({ firstName: 'John', lastName: 'Doe' })
  })

  it('handles nested objects recursively', () => {
    const result = toCamelCaseKeys({ user_profile: { first_name: 'John' } })
    expect(result).toEqual({ userProfile: { firstName: 'John' } })
  })

  it('handles arrays of objects', () => {
    const result = toCamelCaseKeys({ items: [{ item_name: 'foo' }, { item_name: 'bar' }] })
    expect(result).toEqual({ items: [{ itemName: 'foo' }, { itemName: 'bar' }] })
  })

  it('handles null values', () => {
    const result = toCamelCaseKeys({ first_name: null })
    expect(result).toEqual({ firstName: null })
  })

  it('returns an empty object for empty input', () => {
    const result = toCamelCaseKeys({})
    expect(result).toEqual({})
  })

  it('ignores primitive values', () => {
    const result = toCamelCaseKeys({ value: 42, active: true })
    expect(result).toEqual({ value: 42, active: true })
  })
})
