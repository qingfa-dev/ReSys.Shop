import type { Result } from '@/core/models/result'
import type { AccountSettingsResponse } from '../../types/response'
import type { IAccountSettingsRepository } from './account-settings.repository.interface'

class AccountSettingsApiRepository implements IAccountSettingsRepository {
  async get(): Promise<Result<AccountSettingsResponse>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Not implemented' }
  }

  async update(_settings: Partial<AccountSettingsResponse>): Promise<Result<AccountSettingsResponse>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Not implemented' }
  }

  async delete(): Promise<Result<void>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Not implemented' }
  }

  async exportData(): Promise<Result<string>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Not implemented' }
  }
}

export const accountSettingsApiRepository = new AccountSettingsApiRepository()