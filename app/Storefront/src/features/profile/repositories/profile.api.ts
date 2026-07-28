import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { ProfileResponse } from '../types/response'
import type { IProfileRepository } from './profile.repository.interface'
import { PROFILE_ENDPOINTS } from '../types/constants'

export class ProfileApiRepository extends BaseRepository implements IProfileRepository {
  async getProfile(userId: string): Promise<Result<ProfileResponse>> {
    return this.get<ProfileResponse>(`${PROFILE_ENDPOINTS.GET_PROFILE}/${userId}`)
  }

  async updateProfile(userId: string, updates: Partial<ProfileResponse>): Promise<Result<ProfileResponse>> {
    return this.patchPartial<ProfileResponse>(PROFILE_ENDPOINTS.UPDATE_PROFILE, userId, updates)
  }

  async uploadAvatar(userId: string, file: File): Promise<Result<ProfileResponse>> {
    const result = await this.uploadFile(PROFILE_ENDPOINTS.UPDATE_PROFILE, userId, file, 'avatar')
    if (result.isFailure) {
      return result as unknown as Result<ProfileResponse>
    }
    return this.getProfile(userId)
  }
}

export const profileApiRepository = new ProfileApiRepository()
