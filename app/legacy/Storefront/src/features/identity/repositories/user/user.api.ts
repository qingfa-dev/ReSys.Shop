import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { UserResponse } from '../../types/response'
import type { IUserRepository } from './user.repository.interface'

export class UserApiRepository extends BaseRepository implements IUserRepository {
  async getProfile(userId: string): Promise<Result<UserResponse>> {
    return this.get<UserResponse>(`/api/storefront/profiles/profiles`)
  }

  async update(userId: string, updates: Partial<UserResponse>): Promise<Result<UserResponse>> {
    return this.patchPartial<UserResponse>(`/api/storefront/profiles/profiles`, userId, updates)
  }

  async changePassword(userId: string, currentPassword: string, newPassword: string): Promise<Result<void>> {
    return this.post<void>('/api/storefront/identity/passwords/change', { userId, currentPassword, newPassword })
  }

  async requestPasswordReset(email: string): Promise<Result<void>> {
    return this.post<void>('/api/storefront/identity/passwords/forgot', { email })
  }
}

export const userApiRepository = new UserApiRepository()