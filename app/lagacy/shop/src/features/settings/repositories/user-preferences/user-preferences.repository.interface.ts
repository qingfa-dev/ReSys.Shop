import type { Result } from '@/core/models/result'
import type { UserPreferencesResponse } from '../../types/response'

export interface IUserPreferencesRepository {
  get(): Promise<Result<UserPreferencesResponse>>
  update(preferences: Partial<UserPreferencesResponse>): Promise<Result<UserPreferencesResponse>>
}