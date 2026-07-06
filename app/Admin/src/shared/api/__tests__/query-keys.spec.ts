import { describe, it, expect } from 'vitest'
import { withFilters, withId } from '../query-keys'

describe('query-keys helpers', () => {
  it('withFilters appends a filters tuple', () => {
    expect(withFilters(['users', 'list'], { role: 'admin' })).toEqual([
      'users',
      'list',
      { role: 'admin' },
    ])
  })

  it('withId appends a string id', () => {
    expect(withId(['users'], '123')).toEqual(['users', '123'])
  })
})
