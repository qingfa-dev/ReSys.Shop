import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { ProfileResponse } from '../types/response'
import type { IProfileRepository } from './profile.repository.interface'
import { PROFILE_ENDPOINTS } from '../types/constants'

export class ProfileApiRepository extends BaseRepository implements IProfileRepository {
  async getProfile(_userId: string): Promise<Result<ProfileResponse>> {
    return this.get<ProfileResponse>(PROFILE_ENDPOINTS.GET_PROFILE)
  }

  async updateProfile(_userId: string, updates: Partial<ProfileResponse>): Promise<Result<ProfileResponse>> {
    return this.put<ProfileResponse>(PROFILE_ENDPOINTS.UPDATE_PROFILE, updates)
  }

  async uploadAvatar(_userId: string, _file: File): Promise<Result<ProfileResponse>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Avatar upload not available via API' }
  }
}

export const profileApiRepository = new ProfileApiRepository()
