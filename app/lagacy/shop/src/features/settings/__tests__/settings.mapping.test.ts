import { describe, it, expect } from 'vitest'
import {
  toUserPreferences,
  fromUserPreferences,
  toAccountSettings,
  fromAccountSettings,
  getFullName,
  getInitials,
} from '../mapping/settings.mapping'
import { UserPreferencesSchema, AccountSettingsSchema } from '../types/schemas'

describe('Settings Mapping', () => {
  describe('toUserPreferences', () => {
    it('should convert schema to entity', () => {
      const schema = UserPreferencesSchema.parse({
        currency: 'USD',
        language: 'en',
        timezone: 'America/New_York',
        newsletter: true,
      })
      const result = toUserPreferences(schema)
      expect(result.currency).toBe('USD')
      expect(result.language).toBe('en')
    })

    it('should handle missing notifications', () => {
      const schema = UserPreferencesSchema.parse({
        currency: 'USD',
        language: 'en',
        timezone: 'America/New_York',
        newsletter: true,
      })
      const result = toUserPreferences(schema)
      expect(result.notifications).toBeDefined()
    })
  })

  describe('toAccountSettings', () => {
    it('should convert schema to entity', () => {
      const schema = AccountSettingsSchema.parse({
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
      })
      const result = toAccountSettings(schema)
      expect(result.email).toBe('test@example.com')
      expect(result.firstName).toBe('John')
    })
  })

  describe('getFullName', () => {
    it('should return full name', () => {
      const settings = { email: 'test@example.com', firstName: 'John', lastName: 'Doe' }
      expect(getFullName(settings)).toBe('John Doe')
    })
  })

  describe('getInitials', () => {
    it('should return initials', () => {
      const settings = { email: 'test@example.com', firstName: 'John', lastName: 'Doe' }
      expect(getInitials(settings)).toBe('JD')
    })

    it('should handle single name', () => {
      const settings = { email: 'test@example.com', firstName: 'John', lastName: '' }
      expect(getInitials(settings)).toBe('J')
    })
  })
})