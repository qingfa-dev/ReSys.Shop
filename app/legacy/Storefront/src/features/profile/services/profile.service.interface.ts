import type { Result } from '@/core/models/result'
import type { Profile } from '../types/entity'
import type { UpdateProfileRequest } from '../types/request'

export interface IProfileService {
  getProfile(userId: string): Promise<Result<Profile>>
  updateProfile(userId: string, updates: UpdateProfileRequest): Promise<Result<Profile>>
  uploadAvatar(userId: string, file: File): Promise<Result<Profile>>
}
