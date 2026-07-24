import type { UserPreferencesResponse } from '../../types/response'
import type { IUserPreferencesRepository } from './user-preferences.repository.interface'
import type { Result } from '@/core/models/result'

const initialPreferences: UserPreferencesResponse = {
  currency: 'USD',
  language: 'en',
  timezone: 'America/New_York',
  newsletter: false,
}

let mockPreferences: UserPreferencesResponse = { ...initialPreferences }

export class MockUserPreferencesRepository implements IUserPreferencesRepository {
  static reset() {
    mockPreferences = { ...initialPreferences }
  }

  async get(): Promise<Result<UserPreferencesResponse>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockPreferences }
  }

  async update(preferences: Partial<UserPreferencesResponse>): Promise<Result<UserPreferencesResponse>> {
    mockPreferences = { ...mockPreferences, ...preferences }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockPreferences }
  }
}

export const mockUserPreferencesRepository = new MockUserPreferencesRepository()