import { describe, it, expect } from 'vitest'
import {
  emailField,
  credentialField,
  passwordField,
  newPasswordField,
  loginSchema,
  forgotPasswordSchema,
  resetPasswordSchema,
} from '../auth'

describe('emailField', () => {
  it('accepts a valid email', () => {
    expect(emailField.safeParse('test@example.com').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(emailField.safeParse('').success).toBe(false)
  })

  it('rejects invalid email format', () => {
    expect(emailField.safeParse('not-an-email').success).toBe(false)
  })
})

describe('credentialField', () => {
  it('accepts a non-empty string', () => {
    expect(credentialField.safeParse('admin').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(credentialField.safeParse('').success).toBe(false)
  })
})

describe('passwordField', () => {
  it('accepts a non-empty string', () => {
    expect(passwordField.safeParse('secret').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(passwordField.safeParse('').success).toBe(false)
  })
})

describe('newPasswordField', () => {
  it('accepts password with 8+ characters', () => {
    expect(newPasswordField.safeParse('password123').success).toBe(true)
  })

  it('rejects password shorter than 8 characters', () => {
    expect(newPasswordField.safeParse('short').success).toBe(false)
  })
})

describe('loginSchema', () => {
  it('accepts valid credential and password', () => {
    const result = loginSchema.safeParse({ credential: 'admin', password: 'pass' })
    expect(result.success).toBe(true)
  })

  it('rejects empty credential', () => {
    const result = loginSchema.safeParse({ credential: '', password: 'pass' })
    expect(result.success).toBe(false)
  })

  it('rejects empty password', () => {
    const result = loginSchema.safeParse({ credential: 'admin', password: '' })
    expect(result.success).toBe(false)
  })

  it('returns error messages on the correct fields', () => {
    const result = loginSchema.safeParse({ credential: '', password: '' })
    if (!result.success) {
      expect(result.error.issues.some(i => i.path[0] === 'credential')).toBe(true)
      expect(result.error.issues.some(i => i.path[0] === 'password')).toBe(true)
    }
  })
})

describe('forgotPasswordSchema', () => {
  it('accepts valid email', () => {
    const result = forgotPasswordSchema.safeParse({ email: 'user@example.com' })
    expect(result.success).toBe(true)
  })

  it('rejects invalid email', () => {
    const result = forgotPasswordSchema.safeParse({ email: 'bad' })
    expect(result.success).toBe(false)
  })
})

describe('resetPasswordSchema', () => {
  it('accepts valid reset data', () => {
    const result = resetPasswordSchema.safeParse({
      email: 'user@example.com',
      userId: 'abc-123',
      token: 'reset-token-xyz',
      newPassword: 'newpassword123',
    })
    expect(result.success).toBe(true)
  })

  it('rejects when token is empty', () => {
    const result = resetPasswordSchema.safeParse({
      email: 'user@example.com',
      userId: 'abc',
      token: '',
      newPassword: 'newpassword123',
    })
    expect(result.success).toBe(false)
  })

  it('rejects short new password', () => {
    const result = resetPasswordSchema.safeParse({
      email: 'user@example.com',
      userId: 'abc',
      token: 'tok',
      newPassword: 'short',
    })
    expect(result.success).toBe(false)
  })
})
