import type { Result } from '@/core/models/result'
import type { AccountSettingsResponse } from '../../types/response'

export interface IAccountSettingsRepository {
  get(): Promise<Result<AccountSettingsResponse>>
  update(settings: Partial<AccountSettingsResponse>): Promise<Result<AccountSettingsResponse>>
  delete(): Promise<Result<void>>
  exportData(): Promise<Result<string>>
}