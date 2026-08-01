import { describe, it, expect } from 'vitest'
import type { AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { camelCaseInterceptor } from '../interceptors/camelcase'

function mockResponse(data: unknown): AxiosResponse {
  return {
    data,
    status: 200,
    statusText: 'OK',
    headers: { 'content-type': 'application/json' },
    config: {} as InternalAxiosRequestConfig,
  }
}

describe('camelCaseInterceptor', () => {
  it('converts top-level snake_case keys', () => {
    const result = camelCaseInterceptor(mockResponse({ first_name: 'John', last_name: 'Doe' }))
    expect(result.data).toEqual({ firstName: 'John', lastName: 'Doe' })
  })

  it('converts nested objects', () => {
    const result = camelCaseInterceptor(mockResponse({ user_profile: { home_address: { street_name: 'Main' } } }))
    expect(result.data).toEqual({ userProfile: { homeAddress: { streetName: 'Main' } } })
  })

  it('converts arrays of objects', () => {
    const result = camelCaseInterceptor(mockResponse([{ item_name: 'A' }, { item_name: 'B' }]))
    expect(result.data).toEqual([{ itemName: 'A' }, { itemName: 'B' }])
  })

  it('handles null data', () => {
    const result = camelCaseInterceptor(mockResponse(null))
    expect(result.data).toBeNull()
  })

  it('handles primitive data', () => {
    const result = camelCaseInterceptor(mockResponse('hello'))
    expect(result.data).toBe('hello')
  })

  it('handles array of primitives', () => {
    const result = camelCaseInterceptor(mockResponse([1, 2, 3]))
    expect(result.data).toEqual([1, 2, 3])
  })

  it('handles empty object', () => {
    const result = camelCaseInterceptor(mockResponse({}))
    expect(result.data).toEqual({})
  })

  it('does not modify already camelCase keys', () => {
    const result = camelCaseInterceptor(mockResponse({ firstName: 'John' }))
    expect(result.data).toEqual({ firstName: 'John' })
  })
})
