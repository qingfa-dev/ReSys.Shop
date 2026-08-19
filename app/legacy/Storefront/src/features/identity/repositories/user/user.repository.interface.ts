import type { Result } from '@/core/models/result'
import type { UserResponse } from '../../types/response'

export interface IUserRepository {
  getProfile(userId: string): Promise<Result<UserResponse>>
  update(userId: string, updates: Partial<UserResponse>): Promise<Result<UserResponse>>
  changePassword(userId: string, currentPassword: string, newPassword: string): Promise<Result<void>>
  requestPasswordReset(email: string): Promise<Result<void>>
}