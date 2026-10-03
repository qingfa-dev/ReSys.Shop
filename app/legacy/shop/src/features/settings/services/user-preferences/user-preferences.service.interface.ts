import type { Result } from '@/core/models/result'
import type { UserPreferences } from '../../types'

export interface IUserPreferencesService {
  getUserPreferences(): Promise<Result<UserPreferences>>
  updateUserPreferences(preferences: Partial<UserPreferences>): Promise<Result<UserPreferences>>
}