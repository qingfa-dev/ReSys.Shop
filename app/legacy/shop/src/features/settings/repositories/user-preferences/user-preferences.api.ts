import type { Result } from '@/core/models/result'
import type { UserPreferencesResponse } from '../../types/response'
import type { IUserPreferencesRepository } from './user-preferences.repository.interface'

class UserPreferencesApiRepository implements IUserPreferencesRepository {
  async get(): Promise<Result<UserPreferencesResponse>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Not implemented' }
  }

  async update(_preferences: Partial<UserPreferencesResponse>): Promise<Result<UserPreferencesResponse>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Not implemented' }
  }
}

export const userPreferencesApiRepository = new UserPreferencesApiRepository()