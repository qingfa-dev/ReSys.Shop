import { profileRepository } from '../repositories/profile.repository'
import { mapProfileResponse } from '../mappers/profile.mapper'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { Profile } from '../types/Profile.Response.Type'
import type { ProfileUpdateRequest } from '../types/Profile.Request.Type'

function handleResult(result: ServerResult<Profile>): ServerResult<Profile> {
  if (result.isSuccess) {
    return { ...result, value: mapProfileResponse(result.value) }
  }
  return result
}

export const profileService = {
  async getProfile(): Promise<ServerResult<Profile>> {
    const result = await profileRepository.get()
    return handleResult(result)
  },

  async updateProfile(data: ProfileUpdateRequest): Promise<ServerResult<Profile>> {
    const result = await profileRepository.update(data)
    return handleResult(result)
  },
}
