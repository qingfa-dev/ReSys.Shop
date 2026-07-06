import { describe, it, expect } from 'vitest'
import { FilterBuilder } from '../filter-builder'

describe('FilterBuilder', () => {
  it('builds eq condition', () => {
    const result = new FilterBuilder().where('name').eq('John').build()
    expect(result).toBe('name = John')
  })

  it('builds neq condition', () => {
    const result = new FilterBuilder().where('name').neq('John').build()
    expect(result).toBe('name != John')
  })

  it('builds gt condition', () => {
    const result = new FilterBuilder().where('age').gt(18).build()
    expect(result).toBe('age > 18')
  })

  it('builds gte condition', () => {
    const result = new FilterBuilder().where('age').gte('18').build()
    expect(result).toBe('age >= 18')
  })

  it('builds lt condition', () => {
    const result = new FilterBuilder().where('age').lt(18).build()
    expect(result).toBe('age < 18')
  })

  it('builds lte condition', () => {
    const result = new FilterBuilder().where('age').lte('18').build()
    expect(result).toBe('age <= 18')
  })

  it('builds contains condition with wildcard shorthand', () => {
    const result = new FilterBuilder().where('name').contains('John').build()
    expect(result).toBe('name = *John*')
  })

  it('builds starts condition', () => {
    const result = new FilterBuilder().where('name').starts('John').build()
    expect(result).toBe('name = John*')
  })

  it('builds ends condition', () => {
    const result = new FilterBuilder().where('name').ends('John').build()
    expect(result).toBe('name = *John')
  })

  it('chains multiple conditions with AND', () => {
    const result = new FilterBuilder()
      .where('name').eq('John')
      .and()
      .where('age').gt(18)
      .build()
    expect(result).toBe('name = John, age > 18')
  })

  it('chains with OR logic', () => {
    const result = new FilterBuilder()
      .where('status').eq('active')
      .or()
      .where('status').eq('pending')
      .build()
    expect(result).toBe('status = active, status = pending')
  })

  it('returns undefined for empty builder', () => {
    const result = new FilterBuilder().build()
    expect(result).toBeUndefined()
  })

  it('handles numeric value for eq', () => {
    const result = new FilterBuilder().where('age').eq(25).build()
    expect(result).toBe('age = 25')
  })
})
