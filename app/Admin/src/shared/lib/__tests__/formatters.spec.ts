import { describe, it, expect } from 'vitest'
import { formatDate, formatCurrency, formatNumber } from '../formatters'

describe('formatters', () => {
  it('formatDate produces MM/DD/YYYY for en-US locale', () => {
    expect(formatDate(new Date('2026-07-06T10:00:00Z'))).toBe('07/06/2026')
  })
  it('formatCurrency uses USD by default', () => {
    expect(formatCurrency(12.5)).toBe('$12.50')
  })
  it('formatNumber rounds to 2 decimals', () => {
    expect(formatNumber(1.234)).toBe('1.23')
  })
})
