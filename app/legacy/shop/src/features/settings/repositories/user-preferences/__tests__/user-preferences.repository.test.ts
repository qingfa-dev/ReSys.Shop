import { describe, it, expect, beforeEach } from 'vitest'
import { mockUserPreferencesRepository, MockUserPreferencesRepository } from '../user-preferences.mock.repository'

describe('UserPreferencesRepository', () => {
  beforeEach(() => {
    MockUserPreferencesRepository.reset()
  })

  describe('get', () => {
    it('should return user preferences', async () => {
      const result = await mockUserPreferencesRepository.get()
      expect(result.isSuccess).toBe(true)
      expect(result.data?.currency).toBe('USD')
      expect(result.data?.language).toBe('en')
    })
  })

  describe('update', () => {
    it('should update preferences', async () => {
      const result = await mockUserPreferencesRepository.update({ currency: 'EUR', language: 'es' })
      expect(result.isSuccess).toBe(true)
      expect(result.data?.currency).toBe('EUR')
      expect(result.data?.language).toBe('es')
    })

    it('should update nested properties', async () => {
      const result = await mockUserPreferencesRepository.update({ notifications: { email: false, sms: true, push: false } })
      expect(result.isSuccess).toBe(true)
    })
  })
})