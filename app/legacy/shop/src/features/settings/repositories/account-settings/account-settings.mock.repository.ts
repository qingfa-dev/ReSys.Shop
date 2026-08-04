import type { AccountSettingsResponse } from '../../types/response'
import type { IAccountSettingsRepository } from './account-settings.repository.interface'
import type { Result } from '@/core/models/result'

const initialSettings: AccountSettingsResponse = {
  email: 'user@example.com',
  firstName: 'John',
  lastName: 'Doe',
}

let mockSettings: AccountSettingsResponse = { ...initialSettings }

export class MockAccountSettingsRepository implements IAccountSettingsRepository {
  static reset() {
    mockSettings = { ...initialSettings }
  }

  async get(): Promise<Result<AccountSettingsResponse>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockSettings }
  }

  async update(settings: Partial<AccountSettingsResponse>): Promise<Result<AccountSettingsResponse>> {
    mockSettings = { ...mockSettings, ...settings }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockSettings }
  }

  async delete(): Promise<Result<void>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: undefined }
  }

  async exportData(): Promise<Result<string>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: 'https://exports.example.com/user-data.zip' }
  }
}

export const mockAccountSettingsRepository = new MockAccountSettingsRepository()