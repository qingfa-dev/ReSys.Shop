import { describe, it, expect } from 'vitest'
import { useFormatter } from '../useFormatter'

describe('useFormatter', () => {
  const { formatNumber, truncate } = useFormatter()

  describe('formatNumber', () => {
    it('formats integers with comma separators', () => {
      expect(formatNumber(1234)).toBe('1,234')
    })

    it('formats with decimal places', () => {
      expect(formatNumber(1234.567, 2)).toBe('1,234.57')
    })

    it('returns dash for null or undefined', () => {
      expect(formatNumber(null)).toBe('-')
      expect(formatNumber(undefined)).toBe('-')
    })
  })

  describe('truncate', () => {
    it('truncates long strings', () => {
      expect(truncate('Hello World', 5)).toBe('Hello...')
    })

    it('does not truncate short strings', () => {
      expect(truncate('Hello', 10)).toBe('Hello')
    })

    it('handles null or undefined', () => {
      expect(truncate(null, 10)).toBe('')
      expect(truncate(undefined, 5)).toBe('')
    })
  })
})
