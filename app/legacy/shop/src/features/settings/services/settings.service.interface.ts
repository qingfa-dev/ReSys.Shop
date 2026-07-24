import type { Result } from '@/core/models/result'
import type { UserPreferences, AccountSettings } from '../types'

export interface ISettingsService {
  getUserPreferences(): Promise<Result<UserPreferences>>
  updateUserPreferences(preferences: Partial<UserPreferences>): Promise<Result<UserPreferences>>
  getAccountSettings(): Promise<Result<AccountSettings>>
  updateAccountSettings(settings: Partial<AccountSettings>): Promise<Result<AccountSettings>>
  deleteAccount(): Promise<Result<void>>
  exportData(): Promise<Result<string>>
}