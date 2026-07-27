import { describe, it, expect } from 'vitest'
import { formatCurrency, parseCurrency } from './currency'

describe('formatCurrency', () => {
  it('formats a positive number', () => {
    expect(formatCurrency(1234.5)).toBe('$1,234.50')
  })

  it('formats zero', () => {
    expect(formatCurrency(0)).toBe('$0.00')
  })

  it('returns $0.00 for null', () => {
    expect(formatCurrency(null)).toBe('$0.00')
  })

  it('returns $0.00 for undefined', () => {
    expect(formatCurrency(undefined)).toBe('$0.00')
  })

  it('supports custom currency and locale', () => {
    expect(formatCurrency(1234.5, 'EUR', 'de-DE')).toBe('1.234,50\xa0€')
  })
})

describe('parseCurrency', () => {
  it('parses a formatted currency string', () => {
    expect(parseCurrency('$1,234.50')).toBe(1234.5)
  })

  it('parses a string with currency symbol', () => {
    expect(parseCurrency('€99.99')).toBe(99.99)
  })

  it('returns 0 for non-numeric string', () => {
    expect(parseCurrency('abc')).toBe(0)
  })

  it('handles empty string', () => {
    expect(parseCurrency('')).toBe(0)
  })
})
