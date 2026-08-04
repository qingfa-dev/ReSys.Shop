import { profileApiRepository } from '../repositories/profile.api'
import type { IProfileService } from './profile.service.interface'
import type { Profile } from '../types/entity'
import type { UpdateProfileRequest } from '../types/request'
import type { ProfileResponse } from '../types/response'
import type { Result } from '@/core/models/result'
import { mapResponseToEntity } from '../mapping'
import { resultMap } from '@/core/utils/result-helpers'

export class ProfileService implements IProfileService {
  private readonly profileRepository = profileApiRepository

  async getProfile(userId: string): Promise<Result<Profile>> {
    const response = await this.profileRepository.getProfile(userId)
    return resultMap(response, mapResponseToEntity)
  }

  async updateProfile(userId: string, updates: UpdateProfileRequest): Promise<Result<Profile>> {
    const snakeUpdates: Partial<ProfileResponse> = {}
    if (updates.firstName !== undefined) snakeUpdates.first_name = updates.firstName
    if (updates.lastName !== undefined) snakeUpdates.last_name = updates.lastName
    if (updates.displayName !== undefined) snakeUpdates.display_name = updates.displayName
    if (updates.phone !== undefined) snakeUpdates.phone = updates.phone
    if (updates.avatar !== undefined) snakeUpdates.avatar = updates.avatar
    if (updates.dateOfBirth !== undefined) snakeUpdates.date_of_birth = updates.dateOfBirth
    if (updates.gender !== undefined) snakeUpdates.gender = updates.gender

    const response = await this.profileRepository.updateProfile(userId, snakeUpdates)
    return resultMap(response, mapResponseToEntity)
  }

  async uploadAvatar(userId: string, file: File): Promise<Result<Profile>> {
    const response = await this.profileRepository.uploadAvatar(userId, file)
    return resultMap(response, mapResponseToEntity)
  }
}

export const profileService = new ProfileService()
