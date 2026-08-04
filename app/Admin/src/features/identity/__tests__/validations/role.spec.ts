import { describe, it, expect } from 'vitest'
import { roleName, roleDescription, roleSchema } from '../../validations/role'

describe('roleName', () => {
  it('accepts a valid name', () => {
    expect(roleName.safeParse('Admin').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(roleName.safeParse('').success).toBe(false)
  })

  it('accepts a name of exactly 64 characters', () => {
    expect(roleName.safeParse('A'.repeat(64)).success).toBe(true)
  })

  it('rejects a name over 64 characters', () => {
    expect(roleName.safeParse('A'.repeat(65)).success).toBe(false)
  })
})

describe('roleDescription', () => {
  it('accepts a description', () => {
    expect(roleDescription.safeParse('desc').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(roleDescription.safeParse(undefined).success).toBe(true)
  })
})

describe('roleSchema', () => {
  it('accepts a valid role form', () => {
    const result = roleSchema.safeParse({
      name: 'Admin',
      description: 'Administrator role',
      presentation: 'Admin',
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing name', () => {
    const result = roleSchema.safeParse({ description: 'desc' })
    expect(result.success).toBe(false)
  })

  it('rejects empty name', () => {
    const result = roleSchema.safeParse({ name: '' })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors for name', () => {
    const result = roleSchema.safeParse({ name: '' })
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('name')
  })
})
