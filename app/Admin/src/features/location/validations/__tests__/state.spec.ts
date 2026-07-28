import { describe, it, expect } from 'vitest'
import {
  stateName,
  stateAbbreviation,
  stateCountryId,
  stateSchema,
} from '../state'

describe('stateName', () => {
  it('accepts a valid name', () => {
    expect(stateName.safeParse('California').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(stateName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(stateName.safeParse('A'.repeat(101)).success).toBe(false)
  })
})

describe('stateAbbreviation', () => {
  it('accepts a valid abbreviation', () => {
    expect(stateAbbreviation.safeParse('CA').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(stateAbbreviation.safeParse('').success).toBe(false)
  })

  it('rejects abbreviation over 10 characters', () => {
    expect(stateAbbreviation.safeParse('CALIFORNIAX').success).toBe(false)
  })
})

describe('stateCountryId', () => {
  it('accepts a valid GUID', () => {
    expect(stateCountryId.safeParse('abc-123-def').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(stateCountryId.safeParse('').success).toBe(false)
  })
})

describe('stateSchema', () => {
  it('accepts valid state form', () => {
    const result = stateSchema.safeParse({
      name: 'California',
      abbreviation: 'CA',
      countryId: 'abc-123',
      isActive: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing required fields', () => {
    const result = stateSchema.safeParse({
      name: '',
      abbreviation: '',
      countryId: '',
      isActive: true,
    })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors', () => {
    const result = stateSchema.safeParse({
      name: '',
      abbreviation: '',
      countryId: '',
      isActive: true,
    })
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('name')
    expect(fields).toContain('abbreviation')
    expect(fields).toContain('countryId')
  })
})
