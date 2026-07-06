import { describe, it, expect } from 'vitest'
import { ref } from 'vue'
import { useQueryString } from '../useQueryString'

describe('useQueryString', () => {
  it('binds a ref to a query param key', () => {
    const value = useQueryString('q', ref('hello'))
    expect(value.value).toBe('hello')
  })
})
