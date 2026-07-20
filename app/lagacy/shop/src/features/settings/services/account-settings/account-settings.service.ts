import { accountSettingsApiRepository } from '../../repositories/account-settings/account-settings.api'
import { mockAccountSettingsRepository } from '../../repositories/account-settings/account-settings.mock.repository'
import type { IAccountSettingsService } from './account-settings.service.interface'
import type { AccountSettings, AccountSettingsResponse } from '../../types'
import type { Result } from '@/core/models/result'
import { toAccountSettings } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class AccountSettingsService implements IAccountSettingsService {
  private readonly accountSettingsRepository = USE_MOCK ? mockAccountSettingsRepository : accountSettingsApiRepository

  async getAccountSettings(): Promise<Result<AccountSettings>> {
    const response = await this.accountSettingsRepository.get()
    return resultMap(response, toAccountSettings)
  }

  async updateAccountSettings(settings: Partial<AccountSettings>): Promise<Result<AccountSettings>> {
    const response = await this.accountSettingsRepository.update(settings as Partial<AccountSettingsResponse>)
    return resultMap(response, toAccountSettings)
  }

  async deleteAccount(): Promise<Result<void>> {
    return (await this.accountSettingsRepository.delete()) as unknown as Result<void>
  }

  async exportData(): Promise<Result<string>> {
    return (await this.accountSettingsRepository.exportData()) as unknown as Result<string>
  }
}

export const accountSettingsService = new AccountSettingsService()
