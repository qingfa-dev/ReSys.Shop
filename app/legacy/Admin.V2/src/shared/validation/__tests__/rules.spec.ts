import { describe, it, expect } from 'vitest'
import { rules } from '../rules'

describe('rules.required', () => {
  const rule = rules.required()
  it('passes for non-empty string', () => expect(rule('hello')).toBe(true))
  it('fails for empty string', () => expect(rule('')).not.toBe(true))
  it('fails for null', () => expect(rule(null)).not.toBe(true))
  it('fails for undefined', () => expect(rule(undefined)).not.toBe(true))
  it('uses custom label in error message', () => {
    const r = rules.required('Name')
    expect(r('')).toContain('Name')
  })
})

describe('rules.minLength', () => {
  const rule = rules.minLength(3)
  it('passes when long enough', () => expect(rule('abc')).toBe(true))
  it('fails when too short', () => expect(rule('ab')).not.toBe(true))
  it('uses custom label and min in message', () => {
    const r = rules.minLength(5, 'Password')
    expect(r('ab')).toContain('Password')
    expect(r('ab')).toContain('5')
  })
})

describe('rules.maxLength', () => {
  const rule = rules.maxLength(5)
  it('passes within limit', () => expect(rule('hello')).toBe(true))
  it('fails over limit', () => expect(rule('toolong')).not.toBe(true))
})

describe('rules.email', () => {
  it('passes valid email', () => expect(rules.email()('test@example.com')).toBe(true))
  it('fails invalid email', () => expect(rules.email()('not-email')).not.toBe(true))
})

describe('rules.min', () => {
  const rule = rules.min(10)
  it('passes when >= min', () => expect(rule(10)).toBe(true))
  it('fails when < min', () => expect(rule(5)).not.toBe(true))
})

describe('rules.max', () => {
  const rule = rules.max(100)
  it('passes when <= max', () => expect(rule(50)).toBe(true))
  it('fails when > max', () => expect(rule(200)).not.toBe(true))
})
