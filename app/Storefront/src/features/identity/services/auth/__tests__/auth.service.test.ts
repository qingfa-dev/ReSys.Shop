import { describe, it, expect } from 'vitest'
import { authService } from '../auth.service'

import type { LoginRequest, RegisterRequest } from '../../../types'

describe('AuthService', () => {
  describe('login', () => {
    it('should login user', async () => {
      const result = await authService.login({ email: 'test@example.com', password: 'password' } as LoginRequest)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('register', () => {
    it('should register user', async () => {
      const result = await authService.register({ email: 'test@example.com', password: 'password', firstName: 'Test', lastName: 'User' } as RegisterRequest)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('logout', () => {
    it('should logout user', async () => {
      const result = await authService.logout()
      expect(result).toBeDefined()
    })
  })

  describe('requestPasswordReset', () => {
    it('should request password reset', async () => {
      const result = await authService.requestPasswordReset('test@example.com')
      expect(result).toBeDefined()
    })
  })
})