import { describe, it, expect } from 'vitest'
import { EMAIL, PHONE, SLUG, URL, STRONG_PASSWORD } from './regex'

describe('EMAIL', () => {
  it('matches valid emails', () => {
    expect(EMAIL.test('user@example.com')).toBe(true)
    expect(EMAIL.test('a.b@c.co')).toBe(true)
  })

  it('rejects invalid emails', () => {
    expect(EMAIL.test('')).toBe(false)
    expect(EMAIL.test('notanemail')).toBe(false)
    expect(EMAIL.test('@example.com')).toBe(false)
  })
})

describe('PHONE', () => {
  it('matches valid phone numbers', () => {
    expect(PHONE.test('+1234567890')).toBe(true)
    expect(PHONE.test('(555) 123-4567')).toBe(true)
  })

  it('rejects invalid phone numbers', () => {
    expect(PHONE.test('')).toBe(false)
    expect(PHONE.test('12')).toBe(false)
  })
})

describe('SLUG', () => {
  it('matches valid slugs', () => {
    expect(SLUG.test('hello-world')).toBe(true)
    expect(SLUG.test('product-123')).toBe(true)
  })

  it('rejects invalid slugs', () => {
    expect(SLUG.test('Hello-World')).toBe(false)
    expect(SLUG.test('-leading')).toBe(false)
    expect(SLUG.test('trailing-')).toBe(false)
  })
})

describe('URL', () => {
  it('matches valid URLs', () => {
    expect(URL.test('https://example.com')).toBe(true)
    expect(URL.test('http://shop.test/path?q=1')).toBe(true)
  })

  it('rejects invalid URLs', () => {
    expect(URL.test('')).toBe(false)
    expect(URL.test('not-a-url')).toBe(false)
  })
})

describe('STRONG_PASSWORD', () => {
  it('matches strong passwords', () => {
    expect(STRONG_PASSWORD.test('Abcdef1!')).toBe(true)
    expect(STRONG_PASSWORD.test('P@ssw0rdLong')).toBe(true)
  })

  it('rejects weak passwords', () => {
    expect(STRONG_PASSWORD.test('short1!')).toBe(false)
    expect(STRONG_PASSWORD.test('alllowercase1!')).toBe(false)
    expect(STRONG_PASSWORD.test('NOLOWERCASE1!')).toBe(false)
    expect(STRONG_PASSWORD.test('NoNumber!')).toBe(false)
    expect(STRONG_PASSWORD.test('NoSpecial1')).toBe(false)
  })
})
