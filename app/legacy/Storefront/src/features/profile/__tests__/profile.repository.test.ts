import { describe, it, expect, beforeEach } from 'vitest'
import { MockProfileRepository } from '../repositories/profile.mock.repository'
import { mockProfiles } from '../data/mock-profiles.data'

describe('MockProfileRepository', () => {
  let repository: MockProfileRepository

  beforeEach(() => {
    repository = new MockProfileRepository()
  })

  describe('getProfile', () => {
    it('should return profile for existing user', async () => {
      const result = await repository.getProfile('profile-1')

      expect(result.isSuccess).toBe(true)
      expect(result.isFailure).toBe(false)
      expect(result.statusCode).toBe(200)
      expect(result.data?.email).toBe('john.doe@example.com')
    })

    it('should return failure for non-existent user', async () => {
      const result = await repository.getProfile('non-existent')

      expect(result.isSuccess).toBe(false)
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
      expect(result.message).toBe('Profile not found')
    })

    it('should return snake_case response fields', async () => {
      const result = await repository.getProfile('profile-1')

      expect(result.data).toBeDefined()
      expect(result.data!.first_name).toBe('John')
      expect(result.data!.last_name).toBe('Doe')
      expect(result.data!.display_name).toBe('John Doe')
      expect(result.data!.created_at).toBeDefined()
      expect(result.data!.updated_at).toBeDefined()
    })
  })

  describe('updateProfile', () => {
    it('should update existing profile', async () => {
      const result = await repository.updateProfile('profile-1', { display_name: 'Johnny Doe' })

      expect(result.isSuccess).toBe(true)
      expect(result.statusCode).toBe(200)
      expect(result.data?.display_name).toBe('Johnny Doe')
    })

    it('should update updated_at timestamp', async () => {
      const result = await repository.updateProfile('profile-1', { first_name: 'Johnny' })

      expect(result.isSuccess).toBe(true)
      expect(result.data?.updated_at).not.toBe(mockProfiles[0].updatedAt)
    })

    it('should return failure for non-existent user', async () => {
      const result = await repository.updateProfile('non-existent', { first_name: 'Test' })

      expect(result.isSuccess).toBe(false)
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })
  })

  describe('uploadAvatar', () => {
    it('should upload avatar for existing profile', async () => {
      const file = new File([''], 'avatar.png', { type: 'image/png' })
      const result = await repository.uploadAvatar('profile-1', file)

      expect(result.isSuccess).toBe(true)
      expect(result.statusCode).toBe(200)
      expect(result.data?.avatar).toBeDefined()
    })

    it('should return failure for non-existent user', async () => {
      const file = new File([''], 'avatar.png', { type: 'image/png' })
      const result = await repository.uploadAvatar('non-existent', file)

      expect(result.isSuccess).toBe(false)
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })
  })
})
