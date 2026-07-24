import { describe, it, expect } from 'vitest'
import { formatCurrency, parseCurrency } from '../currency'

describe('formatCurrency', () => {
  it('formats a number as USD', () => {
    expect(formatCurrency(1234.5)).toContain('1,234')
  })

  it('returns $0.00 for null', () => {
    expect(formatCurrency(null)).toBe('$0.00')
  })

  it('returns $0.00 for undefined', () => {
    expect(formatCurrency(undefined)).toBe('$0.00')
  })
})

describe('parseCurrency', () => {
  it('parses a currency string', () => {
    expect(parseCurrency('$1,234.56')).toBe(1234.56)
  })

  it('returns 0 for invalid input', () => {
    expect(parseCurrency('abc')).toBe(0)
  })
})
