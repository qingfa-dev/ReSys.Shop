import type { Result } from '@/core/models/result'
import type { ProfileResponse } from '../types/response'

export interface IProfileRepository {
  getProfile(userId: string): Promise<Result<ProfileResponse>>
  updateProfile(userId: string, updates: Partial<ProfileResponse>): Promise<Result<ProfileResponse>>
  uploadAvatar(userId: string, file: File): Promise<Result<ProfileResponse>>
}
