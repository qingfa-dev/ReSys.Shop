import { describe, it, expect } from 'vitest'
import { formatNumber } from './number'

describe('formatNumber', () => {
  it('formats an integer with default decimals', () => {
    expect(formatNumber(1234)).toBe('1,234')
  })

  it('formats with decimal places', () => {
    expect(formatNumber(1234.567, 2)).toBe('1,234.57')
  })

  it('returns dash for null', () => {
    expect(formatNumber(null)).toBe('-')
  })

  it('returns dash for undefined', () => {
    expect(formatNumber(undefined)).toBe('-')
  })

  it('supports custom locale', () => {
    expect(formatNumber(1234.5, 1, 'de-DE')).toBe('1.234,5')
  })
})
