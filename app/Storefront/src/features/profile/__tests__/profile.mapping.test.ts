import { describe, it, expect } from 'vitest'
import { mapResponseToEntity, mapEntityToResponse } from '../mapping/profile.mapping'
import type { ProfileResponse } from '../types/response'
import type { Profile } from '../types/entity'

describe('Profile Mapping', () => {
  const mockResponse: ProfileResponse = {
    id: 'profile-1',
    email: 'john.doe@example.com',
    first_name: 'John',
    last_name: 'Doe',
    display_name: 'John Doe',
    phone: '+1234567890',
    avatar: 'https://picsum.photos/seed/profile1/200/200',
    date_of_birth: '1990-05-15',
    gender: 'male',
    created_at: '2025-01-15T10:00:00Z',
    updated_at: '2026-04-01T10:00:00Z',
  }

  describe('mapResponseToEntity', () => {
    it('should convert snake_case response to camelCase entity', () => {
      const entity = mapResponseToEntity(mockResponse)

      expect(entity.id).toBe('profile-1')
      expect(entity.email).toBe('john.doe@example.com')
      expect(entity.firstName).toBe('John')
      expect(entity.lastName).toBe('Doe')
      expect(entity.displayName).toBe('John Doe')
      expect(entity.phone).toBe('+1234567890')
      expect(entity.avatar).toBe('https://picsum.photos/seed/profile1/200/200')
      expect(entity.dateOfBirth).toBe('1990-05-15')
      expect(entity.gender).toBe('male')
      expect(entity.createdAt).toBe('2025-01-15T10:00:00Z')
      expect(entity.updatedAt).toBe('2026-04-01T10:00:00Z')
    })

    it('should handle optional fields being undefined', () => {
      const minimalResponse: ProfileResponse = {
        id: 'profile-2',
        email: 'jane@example.com',
        first_name: 'Jane',
        last_name: 'Smith',
        display_name: 'Jane Smith',
        created_at: '2025-01-15T10:00:00Z',
        updated_at: '2026-04-01T10:00:00Z',
      }

      const entity = mapResponseToEntity(minimalResponse)

      expect(entity.phone).toBeUndefined()
      expect(entity.avatar).toBeUndefined()
      expect(entity.dateOfBirth).toBeUndefined()
      expect(entity.gender).toBeUndefined()
    })
  })

  describe('mapEntityToResponse', () => {
    it('should convert camelCase entity to snake_case response', () => {
      const entity: Profile = {
        id: 'profile-1',
        email: 'john.doe@example.com',
        firstName: 'John',
        lastName: 'Doe',
        displayName: 'John Doe',
        phone: '+1234567890',
        avatar: 'https://picsum.photos/seed/profile1/200/200',
        dateOfBirth: '1990-05-15',
        gender: 'male',
        createdAt: '2025-01-15T10:00:00Z',
        updatedAt: '2026-04-01T10:00:00Z',
      }

      const response = mapEntityToResponse(entity)

      expect(response.id).toBe('profile-1')
      expect(response.email).toBe('john.doe@example.com')
      expect(response.first_name).toBe('John')
      expect(response.last_name).toBe('Doe')
      expect(response.display_name).toBe('John Doe')
      expect(response.phone).toBe('+1234567890')
      expect(response.avatar).toBe('https://picsum.photos/seed/profile1/200/200')
      expect(response.date_of_birth).toBe('1990-05-15')
      expect(response.gender).toBe('male')
      expect(response.created_at).toBe('2025-01-15T10:00:00Z')
      expect(response.updated_at).toBe('2026-04-01T10:00:00Z')
    })
  })

  describe('round-trip', () => {
    it('should preserve all fields through entity-to-response-to-entity', () => {
      const entity: Profile = {
        id: 'profile-3',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        displayName: 'Test User',
        phone: '+1987654321',
        avatar: 'https://picsum.photos/seed/test/200/200',
        dateOfBirth: '1995-12-01',
        gender: 'female',
        createdAt: '2025-06-01T08:00:00Z',
        updatedAt: '2026-02-15T12:00:00Z',
      }

      const response = mapEntityToResponse(entity)
      const backToEntity = mapResponseToEntity(response)

      expect(backToEntity).toEqual(entity)
    })
  })
})
