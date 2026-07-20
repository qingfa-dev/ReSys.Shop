import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { UserResponse } from '../../types/response'
import type { IUserRepository } from './user.repository.interface'

export class UserApiRepository extends BaseRepository implements IUserRepository {
  getById<T = UserResponse>(id: string): Promise<Result<T>> {
    return super.getById<T>('/identity/users', id)
  }

  async getProfile(userId: string): Promise<Result<UserResponse>> {
    return this.get<UserResponse>(`/identity/users/${userId}`)
  }

  async update(userId: string, updates: Partial<UserResponse>): Promise<Result<UserResponse>> {
    return this.patchPartial<UserResponse>(`/identity/users/${userId}`, userId, updates)
  }

  async changePassword(userId: string, currentPassword: string, newPassword: string): Promise<Result<void>> {
    return this.post<void>('/identity/auth/change-password', { userId, currentPassword, newPassword })
  }

  async requestPasswordReset(email: string): Promise<Result<void>> {
    return this.post<void>('/identity/auth/forgot-password', { email })
  }

  async enableMFA(userId: string): Promise<Result<{ secret: string; qrCode: string }>> {
    return this.post<{ secret: string; qrCode: string }>('/identity/auth/mfa/enable', { userId })
  }

  async verifyMFA(userId: string, code: string): Promise<Result<void>> {
    return this.post<void>('/identity/auth/mfa/verify', { userId, code })
  }

  async disableMFA(userId: string, code: string): Promise<Result<void>> {
    return this.post<void>('/identity/auth/mfa/disable', { userId, code })
  }
}

export const userApiRepository = new UserApiRepository()