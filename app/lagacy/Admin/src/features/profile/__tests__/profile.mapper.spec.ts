import { describe, it, expect } from 'vitest'
import { mapProfileResponse } from '../models/profile.mapper'

describe('mapProfileResponse', () => {
  it('maps all fields', () => {
    const dto = {
      id: 'u1', email: 'a@b.com', firstName: 'John', lastName: 'Doe',
      phoneNumber: undefined, dateOfBirth: undefined, gender: undefined,
      bio: undefined, avatarUrl: undefined, preferences: {},
      notifications: {}, isActive: true, acceptsEmailMarketing: false,
      createdAtUtc: '2025-01-01T00:00:00Z', modifiedAtUtc: undefined,
    }
    const result = mapProfileResponse(dto)
    expect(result.id).toBe('u1')
    expect(result.email).toBe('a@b.com')
    expect(result.firstName).toBe('John')
    expect(result.isActive).toBe(true)
  })
})
