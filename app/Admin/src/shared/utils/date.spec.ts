import { describe, it, expect } from 'vitest'
import { formatDate } from './date'

describe('formatDate', () => {
  it('formats a Date object', () => {
    const date = new Date(2025, 0, 15)
    expect(formatDate(date)).toBe('1/15/2025')
  })

  it('formats an ISO string', () => {
    expect(formatDate('2025-06-01T12:00:00Z')).toBe('6/1/2025')
  })

  it('returns dash for null', () => {
    expect(formatDate(null)).toBe('-')
  })

  it('returns dash for undefined', () => {
    expect(formatDate(undefined)).toBe('-')
  })

  it('returns dash for invalid date string', () => {
    expect(formatDate('not-a-date')).toBe('-')
  })

  it('supports custom options and locale', () => {
    const date = new Date(2025, 0, 15)
    const options: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }
    expect(formatDate(date, options, 'de-DE')).toBe('15. Januar 2025')
  })
})
