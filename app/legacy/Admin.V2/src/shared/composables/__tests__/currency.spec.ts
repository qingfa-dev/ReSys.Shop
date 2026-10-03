import { describe, it, expect, vi } from 'vitest'
import { useCurrency } from '../useCurrency'

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    locale: { value: 'en-US' },
  }),
}))

describe('useCurrency', () => {
  it('formats a number as USD by default', () => {
    const { format } = useCurrency()
    expect(format(1234.5)).toContain('1,234')
  })

  it('supports different currencies', () => {
    const { format } = useCurrency()
    const result = format(100, 'EUR')
    expect(result).toContain('100')
  })

  it('returns $0.00 for null', () => {
    const { format } = useCurrency()
    expect(format(null)).toBe('$0.00')
  })

  it('returns $0.00 for undefined', () => {
    const { format } = useCurrency()
    expect(format(undefined)).toBe('$0.00')
  })
})
