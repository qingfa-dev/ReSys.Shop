import { describe, it, expect } from 'vitest'
import { camelCaseInterceptor } from '../interceptors/camelcase'

describe('camelCaseInterceptor', () => {
  it('converts top-level snake_case keys', () => {
    const response = { data: { first_name: 'John', last_name: 'Doe' } }
    const result = camelCaseInterceptor(response)
    expect(result.data).toEqual({ firstName: 'John', lastName: 'Doe' })
  })

  it('converts nested objects', () => {
    const response = { data: { user_profile: { home_address: { street_name: 'Main' } } } }
    const result = camelCaseInterceptor(response)
    expect(result.data).toEqual({ userProfile: { homeAddress: { streetName: 'Main' } } })
  })

  it('converts arrays of objects', () => {
    const response = { data: [{ item_name: 'A' }, { item_name: 'B' }] }
    const result = camelCaseInterceptor(response)
    expect(result.data).toEqual([{ itemName: 'A' }, { itemName: 'B' }])
  })

  it('handles null data', () => {
    const response = { data: null }
    const result = camelCaseInterceptor(response)
    expect(result.data).toBeNull()
  })

  it('handles primitive data', () => {
    const response = { data: 'hello' }
    const result = camelCaseInterceptor(response)
    expect(result.data).toBe('hello')
  })

  it('handles array of primitives', () => {
    const response = { data: [1, 2, 3] }
    const result = camelCaseInterceptor(response)
    expect(result.data).toEqual([1, 2, 3])
  })

  it('handles empty object', () => {
    const response = { data: {} }
    const result = camelCaseInterceptor(response)
    expect(result.data).toEqual({})
  })

  it('does not modify already camelCase keys', () => {
    const response = { data: { firstName: 'John' } }
    const result = camelCaseInterceptor(response)
    expect(result.data).toEqual({ firstName: 'John' })
  })
})
