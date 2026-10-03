import { describe, it, expect } from 'vitest'
import {
  userEmail,
  userUserName,
  userFirstName,
  userLastName,
  userPhoneNumber,
  userEmailConfirmed,
  userPhoneNumberConfirmed,
  userSchema,
} from '../../validations/user'

describe('userEmail', () => {
  it('accepts a valid email', () => {
    expect(userEmail.safeParse('a@b.com').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(userEmail.safeParse('').success).toBe(false)
  })

  it('rejects an invalid email', () => {
    expect(userEmail.safeParse('not-an-email').success).toBe(false)
  })
})

describe('userUserName', () => {
  it('accepts a valid username', () => {
    expect(userUserName.safeParse('admin').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(userUserName.safeParse('').success).toBe(false)
  })
})

describe('userFirstName', () => {
  it('accepts a first name', () => {
    expect(userFirstName.safeParse('A').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(userFirstName.safeParse('').success).toBe(false)
  })
})

describe('userLastName', () => {
  it('accepts a last name', () => {
    expect(userLastName.safeParse('A').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(userLastName.safeParse('').success).toBe(false)
  })
})

describe('userPhoneNumber', () => {
  it('accepts a phone number', () => {
    expect(userPhoneNumber.safeParse('123').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(userPhoneNumber.safeParse(undefined).success).toBe(true)
  })
})

describe('userEmailConfirmed', () => {
  it('accepts true', () => {
    expect(userEmailConfirmed.safeParse(true).success).toBe(true)
  })

  it('accepts false', () => {
    expect(userEmailConfirmed.safeParse(false).success).toBe(true)
  })
})

describe('userPhoneNumberConfirmed', () => {
  it('accepts true', () => {
    expect(userPhoneNumberConfirmed.safeParse(true).success).toBe(true)
  })

  it('accepts false', () => {
    expect(userPhoneNumberConfirmed.safeParse(false).success).toBe(true)
  })
})

describe('userSchema', () => {
  it('accepts a valid user form', () => {
    const result = userSchema.safeParse({
      email: 'a@b.com',
      userName: 'admin',
      firstName: 'A',
      lastName: 'B',
      phoneNumber: '123',
      emailConfirmed: true,
      phoneNumberConfirmed: false,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing email', () => {
    const result = userSchema.safeParse({
      userName: 'admin',
      firstName: 'A',
      lastName: 'B',
      emailConfirmed: true,
      phoneNumberConfirmed: false,
    })
    expect(result.success).toBe(false)
  })
})
