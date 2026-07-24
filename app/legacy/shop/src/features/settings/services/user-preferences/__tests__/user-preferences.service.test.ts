import { describe, it, expect } from 'vitest'
import { userPreferencesService } from '../user-preferences.service'

import type { UserPreferences } from '../../../types'

describe('UserPreferencesService', () => {
  describe('getUserPreferences', () => {
    it('should return user preferences', async () => {
      const result = await userPreferencesService.getUserPreferences()
      expect(result).toBeDefined()
    })
  })

  describe('updateUserPreferences', () => {
    it('should update user preferences', async () => {
      const result = await userPreferencesService.updateUserPreferences({ theme: 'dark' } as Partial<UserPreferences>)
      expect(result).toBeDefined()
    })
  })
})