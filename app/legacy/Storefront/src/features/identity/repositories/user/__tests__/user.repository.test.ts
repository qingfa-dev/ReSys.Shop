import { describe, it, expect } from 'vitest'
import { mockUserRepository } from '../user.mock.repository'

describe('UserRepository', () => {
  describe('getById', () => {
    it('should return user by id', async () => {
      const result = await mockUserRepository.getById('user-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getProfile', () => {
    it('should return user profile', async () => {
      const result = await mockUserRepository.getProfile('user-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('updateProfile', () => {
    it('should update user profile', async () => {
      const result = await mockUserRepository.updateProfile('user-1', { firstName: 'John' })
      expect(result.isSuccess).toBe(true)
    })
  })
})