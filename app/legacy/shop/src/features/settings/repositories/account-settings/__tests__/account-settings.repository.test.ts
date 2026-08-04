import { describe, it, expect, beforeEach } from 'vitest'
import { mockAccountSettingsRepository, MockAccountSettingsRepository } from '../account-settings.mock.repository'

describe('AccountSettingsRepository', () => {
  beforeEach(() => {
    MockAccountSettingsRepository.reset()
  })

  describe('get', () => {
    it('should return account settings', async () => {
      const result = await mockAccountSettingsRepository.get()
      expect(result.isSuccess).toBe(true)
      expect(result.data?.email).toBe('user@example.com')
    })
  })

  describe('update', () => {
    it('should update account settings', async () => {
      const result = await mockAccountSettingsRepository.update({ phone: '+1987654321' })
      expect(result.isSuccess).toBe(true)
      expect(result.data?.phone).toBe('+1987654321')
    })
  })

  describe('delete', () => {
    it('should delete account', async () => {
      const result = await mockAccountSettingsRepository.delete()
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('exportData', () => {
    it('should return export URL', async () => {
      const result = await mockAccountSettingsRepository.exportData()
      expect(result.isSuccess).toBe(true)
      expect(result.data).toContain('exports.example.com')
    })
  })
})