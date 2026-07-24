import type { Result } from '@/core/models/result'
import type { UserResponse } from '../../types/response'

export interface IUserRepository {
  getById<T = UserResponse>(id: string): Promise<Result<T>>
  getProfile(userId: string): Promise<Result<UserResponse>>
  update(userId: string, updates: Partial<UserResponse>): Promise<Result<UserResponse>>
  changePassword(userId: string, currentPassword: string, newPassword: string): Promise<Result<void>>
  requestPasswordReset(email: string): Promise<Result<void>>
  enableMFA(userId: string): Promise<Result<{ secret: string; qrCode: string }>>
  verifyMFA(userId: string, code: string): Promise<Result<void>>
  disableMFA(userId: string, code: string): Promise<Result<void>>
}