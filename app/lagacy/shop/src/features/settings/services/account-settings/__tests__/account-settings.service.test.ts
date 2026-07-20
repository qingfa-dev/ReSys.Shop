import { describe, it, expect } from 'vitest'
import { accountSettingsService } from '../account-settings.service'

import type { AccountSettings } from '../../../types'

describe('AccountSettingsService', () => {
  describe('getAccountSettings', () => {
    it('should return account settings', async () => {
      const result = await accountSettingsService.getAccountSettings()
      expect(result).toBeDefined()
    })
  })

  describe('updateAccountSettings', () => {
    it('should update account settings', async () => {
      const result = await accountSettingsService.updateAccountSettings({ emailNotifications: false } as Partial<AccountSettings>)
      expect(result).toBeDefined()
    })
  })

  describe('deleteAccount', () => {
    it('should delete account', async () => {
      const result = await accountSettingsService.deleteAccount()
      expect(result).toBeDefined()
    })
  })

  describe('exportData', () => {
    it('should export data', async () => {
      const result = await accountSettingsService.exportData()
      expect(result).toBeDefined()
    })
  })
})