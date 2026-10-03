import { describe, it, expect } from 'vitest'
import { mockAuthRepository } from '../auth.mock.repository'

describe('AuthRepository', () => {
  describe('login', () => {
    it('should login with valid credentials', async () => {
      const result = await mockAuthRepository.login({ email: 'test@example.com', password: 'password123' })
      expect(result.isSuccess).toBe(true)
    })

    it('should fail with invalid credentials', async () => {
      const result = await mockAuthRepository.login({ email: '', password: '' })
      expect(result.isFailure).toBe(true)
    })
  })

  describe('register', () => {
    it('should register with valid data', async () => {
      const result = await mockAuthRepository.register({ email: 'new@example.com', password: 'password123', firstName: 'Jane', lastName: 'Doe' })
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('logout', () => {
    it('should logout successfully', async () => {
      const result = await mockAuthRepository.logout()
      expect(result.isSuccess).toBe(true)
    })
  })
})