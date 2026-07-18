import { profileRepository } from '../api/profile.api'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { Profile } from '../types/profile.response.type'
import type { ProfileUpdateRequest } from '../types/profile.request.type'

export const profileService = {
  async getProfile(): Promise<ServerResult<Profile>> {
    return profileRepository.get()
  },

  async updateProfile(data: ProfileUpdateRequest): Promise<ServerResult<Profile>> {
    return profileRepository.update(data)
  },
}
