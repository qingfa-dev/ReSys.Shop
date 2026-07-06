import { describe, it, expect } from 'vitest'
import { loginSchema } from '../../model/auth.schema'

describe('loginSchema', () => {
  it('accepts valid input', () => {
    expect(loginSchema.safeParse({ email: 'a@b.co', password: 'secret123' }).success).toBe(true)
  })
  it('rejects missing email', () => {
    expect(loginSchema.safeParse({ email: '', password: 'secret123' }).success).toBe(false)
  })
  it('rejects short password', () => {
    expect(loginSchema.safeParse({ email: 'a@b.co', password: 'short' }).success).toBe(false)
  })
})
