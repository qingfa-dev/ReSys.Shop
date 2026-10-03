import { userApiRepository } from '../../repositories/user/user.api'
import type { IUserService } from './user.service.interface'
import type { User, UpdateProfileRequest, UserResponse } from '../../types'
import type { Result } from '@/core/models/result'
import { mapResponseToEntity } from '../../mapping'
import { resultMap, succeed, fail } from '@/core/utils/result-helpers'

export class UserService implements IUserService {
  private readonly userRepository = userApiRepository

  async getProfile(userId: string): Promise<Result<User>> {
    const response = await this.userRepository.getProfile(userId)
    return resultMap(response, mapResponseToEntity)
  }

  async updateProfile(userId: string, updates: UpdateProfileRequest): Promise<Result<User>> {
    const response = await this.userRepository.update(userId, updates as Partial<UserResponse>)
    return resultMap(response, mapResponseToEntity)
  }

  async changePassword(
    userId: string,
    currentPassword: string,
    newPassword: string,
  ): Promise<Result<void>> {
    const response = await this.userRepository.changePassword(
      userId,
      currentPassword,
      newPassword,
    ) as Result<void>
    if (response.isFailure) {
      return fail(response.message ?? 'Password change failed', response.statusCode, response.errors)
    }
    return succeed(undefined, response.statusCode)
  }

  async requestPasswordReset(email: string): Promise<Result<void>> {
    const response = await this.userRepository.requestPasswordReset(email) as Result<void>
    if (response.isFailure) {
      return fail(response.message ?? 'Password reset failed', response.statusCode, response.errors)
    }
    return succeed(undefined, response.statusCode)
  }
}

export const userService = new UserService()