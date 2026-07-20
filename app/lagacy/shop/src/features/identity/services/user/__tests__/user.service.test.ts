import { describe, it, expect } from 'vitest'
import { userService } from '../user.service'

import type { UpdateProfileRequest } from '../../../types'

describe('UserService', () => {
  describe('getProfile', () => {
    it('should return user profile', async () => {
      const result = await userService.getProfile('user-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('updateProfile', () => {
    it('should update user profile', async () => {
      const result = await userService.updateProfile('user-1', { firstName: 'John' } as UpdateProfileRequest)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('changePassword', () => {
    it('should change user password', async () => {
      const result = await userService.changePassword('user-1', 'oldPassword', 'newPassword')
      expect(result).toBeDefined()
    })
  })

  describe('requestPasswordReset', () => {
    it('should request password reset', async () => {
      const result = await userService.requestPasswordReset('test@example.com')
      expect(result).toBeDefined()
    })
  })
})