import { describe, it, expect, vi } from 'vitest'
import { useDate } from '../useDate'

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    locale: { value: 'en-US' },
  }),
}))

describe('useDate', () => {
  it('format returns formatted date string', () => {
    const { format } = useDate()
    const result = format('2025-01-15')
    expect(result).not.toBe('-')
    expect(typeof result).toBe('string')
  })

  it('format returns "-" for null/undefined', () => {
    const { format } = useDate()
    expect(format(null)).toBe('-')
    expect(format(undefined)).toBe('-')
  })

  it('formatRelative returns "just now" for recent dates', () => {
    const { formatRelative } = useDate()
    const now = new Date()
    expect(formatRelative(now)).toBe('just now')
  })

  it('formatRelative returns "-" for null/undefined', () => {
    const { formatRelative } = useDate()
    expect(formatRelative(null)).toBe('-')
    expect(formatRelative(undefined)).toBe('-')
  })

  it('formatRelative returns relative time for older dates', () => {
    const { formatRelative } = useDate()
    const hoursAgo = new Date(Date.now() - 3 * 60 * 60 * 1000)
    const result = formatRelative(hoursAgo)
    expect(result).toMatch(/\d+h ago/)
  })

  it('formatRelative returns "Xm ago" for minutes', () => {
    const { formatRelative } = useDate()
    const minsAgo = new Date(Date.now() - 5 * 60 * 1000)
    expect(formatRelative(minsAgo)).toMatch(/\d+m ago/)
  })

  it('formatRelative returns "Xd ago" for days', () => {
    const { formatRelative } = useDate()
    const daysAgo = new Date(Date.now() - 3 * 24 * 60 * 60 * 1000)
    expect(formatRelative(daysAgo)).toMatch(/\d+d ago/)
  })
})
