import { describe, it, expect, vi, afterEach } from 'vitest'
import { formatDate, formatDateTimeUtc, toUtcIso, fromUtcToDateInput } from './date'

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

describe('formatDateTimeUtc', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('formats UTC timestamp in local time zone', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2025-06-01T12:00:00Z'))
    const formatted = formatDateTimeUtc('2025-06-01T12:00:00Z')
    expect(formatted).not.toBe('-')
    expect(formatted).toMatch(/\d{4}/)
  })

  it('returns dash for null/undefined/invalid', () => {
    expect(formatDateTimeUtc(null)).toBe('-')
    expect(formatDateTimeUtc(undefined)).toBe('-')
    expect(formatDateTimeUtc('bad')).toBe('-')
  })
})

describe('toUtcIso', () => {
  it('converts local ISO string to UTC ISO', () => {
    const result = toUtcIso('2025-06-01T12:00:00')
    expect(result).toBe(new Date('2025-06-01T12:00:00').toISOString())
  })

  it('returns null for empty/null/undefined/invalid', () => {
    expect(toUtcIso(null)).toBeNull()
    expect(toUtcIso(undefined)).toBeNull()
    expect(toUtcIso('')).toBeNull()
    expect(toUtcIso('bad')).toBeNull()
  })
})

describe('fromUtcToDateInput', () => {
  it('converts UTC timestamp to local date-only input value', () => {
    const result = fromUtcToDateInput('2025-06-01T00:00:00Z')
    expect(result).toMatch(/^\d{4}-\d{2}-\d{2}$/)
  })

  it('returns null for empty/null/undefined/invalid', () => {
    expect(fromUtcToDateInput(null)).toBeNull()
    expect(fromUtcToDateInput(undefined)).toBeNull()
    expect(fromUtcToDateInput('')).toBeNull()
    expect(fromUtcToDateInput('bad')).toBeNull()
  })
})
