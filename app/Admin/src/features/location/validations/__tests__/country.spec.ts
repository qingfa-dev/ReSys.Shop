import { describe, it, expect } from 'vitest'
import {
  countryName,
  countryIsoCode,
  countryCallingCode,
  countrySchema,
} from '../country'

describe('countryName', () => {
  it('accepts a valid name', () => {
    expect(countryName.safeParse('United States').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(countryName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(countryName.safeParse('A'.repeat(101)).success).toBe(false)
  })

  it('accepts string of exactly 100 characters', () => {
    expect(countryName.safeParse('A'.repeat(100)).success).toBe(true)
  })
})

describe('countryIsoCode', () => {
  it('accepts valid 2-letter code', () => {
    expect(countryIsoCode.safeParse('US').success).toBe(true)
  })

  it('accepts valid 3-letter code', () => {
    expect(countryIsoCode.safeParse('USA').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(countryIsoCode.safeParse('').success).toBe(false)
  })

  it('rejects lowercase', () => {
    expect(countryIsoCode.safeParse('us').success).toBe(false)
  })

  it('rejects single letter', () => {
    expect(countryIsoCode.safeParse('U').success).toBe(false)
  })

  it('rejects 4 letters', () => {
    expect(countryIsoCode.safeParse('USAA').success).toBe(false)
  })

  it('returns correct error message', () => {
    const result = countryIsoCode.safeParse('us')
    if (!result.success) {
      expect(result.error!.issues[0]!.message).toBe('ISO code must be 2-3 uppercase letters.')
    }
  })
})

describe('countryCallingCode', () => {
  it('accepts a valid calling code', () => {
    expect(countryCallingCode.safeParse('+84').success).toBe(true)
  })

  it('rejects code over 10 characters', () => {
    expect(countryCallingCode.safeParse('+12345678901').success).toBe(false)
  })
})

describe('countrySchema', () => {
  it('accepts valid country form', () => {
    const result = countrySchema.safeParse({
      name: 'Vietnam',
      isoCode: 'VN',
      callingCode: '+84',
      statesRequired: true,
      isActive: true,
    })
    expect(result.success).toBe(true)
  })

  it('accepts form without optional callingCode', () => {
    const result = countrySchema.safeParse({
      name: 'Vietnam',
      isoCode: 'VN',
      statesRequired: false,
      isActive: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing required name', () => {
    const result = countrySchema.safeParse({
      name: '',
      isoCode: 'VN',
      statesRequired: false,
      isActive: true,
    })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors', () => {
    const result = countrySchema.safeParse({
      name: '',
      isoCode: 'v',
      statesRequired: false,
      isActive: true,
    })
    if (!result.success) {
      const fields = result.error.issues.map(i => String(i.path[0]))
      expect(fields).toContain('name')
      expect(fields).toContain('isoCode')
    }
  })
})
