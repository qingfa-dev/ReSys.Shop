import { describe, it, expect } from 'vitest'
import { toCamelCase, mapKeys, toCamelCaseKeys } from '../transform'

describe('toCamelCase', () => {
  it('converts snake_case to camelCase', () => {
    expect(toCamelCase('first_name')).toBe('firstName')
  })

  it('lowercases first character', () => {
    expect(toCamelCase('FirstName')).toBe('firstName')
  })
})

describe('mapKeys', () => {
  it('transforms object keys', () => {
    const result = mapKeys({ first_name: 'John' }, k => k.toUpperCase())
    expect(result).toEqual({ FIRST_NAME: 'John' })
  })
})

describe('toCamelCaseKeys', () => {
  it('converts top-level keys', () => {
    const result = toCamelCaseKeys({ first_name: 'John', last_name: 'Doe' })
    expect(result).toEqual({ firstName: 'John', lastName: 'Doe' })
  })

  it('recursively converts nested object keys', () => {
    const result = toCamelCaseKeys({ user_info: { first_name: 'John' } })
    expect(result).toEqual({ userInfo: { firstName: 'John' } })
  })

  it('recursively converts keys in arrays', () => {
    const result = toCamelCaseKeys({ items: [{ item_name: 'Foo' }] })
    expect(result).toEqual({ items: [{ itemName: 'Foo' }] })
  })

  it('handles null values', () => {
    const result = toCamelCaseKeys({ first_name: null })
    expect(result).toEqual({ firstName: null })
  })
})
