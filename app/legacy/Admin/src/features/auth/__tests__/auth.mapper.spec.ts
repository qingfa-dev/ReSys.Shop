import { describe, it, expect } from 'vitest'
import { mapProfileResponse, mapSessionResponse, mapJwtToProfile } from '../models/auth.mapper'

describe('mapProfileResponse', () => {
  it('extracts fields from raw response', () => {
    const result = mapProfileResponse({ id: 'guid-1', email: 'a@b.com', fullName: 'John', roles: ['Admin'] })
    expect(result.id).toBe('guid-1')
    expect(result.email).toBe('a@b.com')
    expect(result.fullName).toBe('John')
    expect(result.roles).toEqual(['Admin'])
  })
  it('falls back for missing fields', () => {
    const result = mapProfileResponse({})
    expect(result.id).toBe('')
    expect(result.email).toBe('')
    expect(result.fullName).toBe('')
    expect(result.roles).toEqual([])
  })
  it('handles PascalCase fallback', () => {
    const result = mapProfileResponse({ Id: 'x', Email: 'y', FullName: 'z', Roles: 'Admin' })
    expect(result.id).toBe('x')
    expect(result.email).toBe('y')
    expect(result.fullName).toBe('z')
    expect(result.roles).toEqual([])
  })
})

describe('mapSessionResponse', () => {
  it('maps id and roles', () => {
    const result = mapSessionResponse({ id: 'g1', roles: ['Admin', 'Staff'] })
    expect(result.id).toBe('g1')
    expect(result.roles).toEqual(['Admin', 'Staff'])
    expect(result.permissions).toEqual([])
  })
})

describe('mapJwtToProfile', () => {
  it('extracts from standard JWT claims', () => {
    const result = mapJwtToProfile({ sub: 'u1', email: 'a@b.com', role: ['Admin'] })
    expect(result.id).toBe('u1')
    expect(result.email).toBe('a@b.com')
    expect(result.roles).toEqual(['Admin'])
  })
})
