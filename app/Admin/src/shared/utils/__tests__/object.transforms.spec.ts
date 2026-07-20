import { describe, it, expect } from 'vitest'
import { mapKeys, toCamelCaseKeys } from '../object.transforms'

describe('mapKeys', () => {
  it('transforms keys of a flat object', () => {
    const result = mapKeys({ foo_bar: 1, baz_qux: 2 }, (k) => k.toUpperCase())
    expect(result).toEqual({ FOO_BAR: 1, BAZ_QUX: 2 })
  })

  it('returns empty object for empty input', () => {
    expect(mapKeys({}, (k) => k)).toEqual({})
  })
})

describe('toCamelCaseKeys', () => {
  it('converts snake_case keys to camelCase', () => {
    const result = toCamelCaseKeys({ user_name: 'john', first_name: 'John', last_name: 'Doe' })
    expect(result).toEqual({ userName: 'john', firstName: 'John', lastName: 'Doe' })
  })

  it('preserves null values', () => {
    const result = toCamelCaseKeys({ foo_bar: null })
    expect(result).toEqual({ fooBar: null })
  })

  it('does not recurse into nested objects', () => {
    const result = toCamelCaseKeys({ outer_key: { inner_key: 'value' } })
    expect(result).toEqual({ outerKey: { inner_key: 'value' } })
  })
})
