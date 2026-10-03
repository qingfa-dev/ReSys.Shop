import type { Result } from '@/core/models/result'
import type { AccountSettings } from '../../types'

export interface IAccountSettingsService {
  getAccountSettings(): Promise<Result<AccountSettings>>
  updateAccountSettings(settings: Partial<AccountSettings>): Promise<Result<AccountSettings>>
  deleteAccount(): Promise<Result<void>>
  exportData(): Promise<Result<string>>
}