import { describe, it, expect } from 'vitest'
import {
  profileFirstName,
  profileLastName,
  profileEmail,
  profilePhoneNumber,
  profileSchema,
} from '../../validations/profile'

describe('profileFirstName', () => {
  it('accepts a single character', () => {
    expect(profileFirstName.safeParse('A').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(profileFirstName.safeParse('').success).toBe(false)
  })

  it('accepts a 100-character name', () => {
    expect(profileFirstName.safeParse('a'.repeat(100)).success).toBe(true)
  })

  it('rejects a 101-character name', () => {
    expect(profileFirstName.safeParse('a'.repeat(101)).success).toBe(false)
  })
})

describe('profileLastName', () => {
  it('accepts a valid last name', () => {
    expect(profileLastName.safeParse('Smith').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(profileLastName.safeParse('').success).toBe(false)
  })

  it('accepts a 100-character name', () => {
    expect(profileLastName.safeParse('a'.repeat(100)).success).toBe(true)
  })

  it('rejects a 101-character name', () => {
    expect(profileLastName.safeParse('a'.repeat(101)).success).toBe(false)
  })
})

describe('profileEmail', () => {
  it('accepts a valid email', () => {
    expect(profileEmail.safeParse('a@b.com').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(profileEmail.safeParse('').success).toBe(false)
  })

  it('rejects a non-email string', () => {
    expect(profileEmail.safeParse('not-email').success).toBe(false)
  })
})

describe('profilePhoneNumber', () => {
  it('accepts a phone number', () => {
    expect(profilePhoneNumber.safeParse('123').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(profilePhoneNumber.safeParse(undefined).success).toBe(true)
  })
})

describe('profileSchema', () => {
  it('accepts a valid profile form', () => {
    const result = profileSchema.safeParse({
      userId: 'u-1',
      firstName: 'A',
      lastName: 'B',
      email: 'a@b.com',
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing firstName', () => {
    const result = profileSchema.safeParse({
      userId: 'u-1',
      lastName: 'B',
      email: 'a@b.com',
    })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors', () => {
    const result = profileSchema.safeParse({})
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('userId')
    expect(fields).toContain('firstName')
    expect(fields).toContain('lastName')
    expect(fields).toContain('email')
  })
})
