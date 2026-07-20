import { userPreferencesApiRepository } from '../../repositories/user-preferences/user-preferences.api'
import { mockUserPreferencesRepository } from '../../repositories/user-preferences/user-preferences.mock.repository'
import type { IUserPreferencesService } from './user-preferences.service.interface'
import type { UserPreferences, UserPreferencesResponse } from '../../types'
import type { Result } from '@/core/models/result'
import { toUserPreferences } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class UserPreferencesService implements IUserPreferencesService {
  private readonly userPreferencesRepository = USE_MOCK ? mockUserPreferencesRepository : userPreferencesApiRepository

  async getUserPreferences(): Promise<Result<UserPreferences>> {
    const response = await this.userPreferencesRepository.get()
    return resultMap(response, toUserPreferences)
  }

  async updateUserPreferences(preferences: Partial<UserPreferences>): Promise<Result<UserPreferences>> {
    const response = await this.userPreferencesRepository.update(preferences as Partial<UserPreferencesResponse>)
    return resultMap(response, toUserPreferences)
  }
}

export const userPreferencesService = new UserPreferencesService()
