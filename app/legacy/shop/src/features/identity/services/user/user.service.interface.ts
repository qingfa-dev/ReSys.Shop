import type { Result } from '@/core/models/result'
import type { User, UpdateProfileRequest } from '../../types'

export interface IUserService {
  getProfile(userId: string): Promise<Result<User>>
  updateProfile(userId: string, updates: UpdateProfileRequest): Promise<Result<User>>
  changePassword(userId: string, currentPassword: string, newPassword: string): Promise<Result<void>>
  requestPasswordReset(email: string): Promise<Result<void>>
}