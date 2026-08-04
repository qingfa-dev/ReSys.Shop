import type { UserResponse, UserSingleResponse } from '../../types/response'
import { getUserById } from '../../data/mock-users.data'

export class MockUserRepository {
  async getProfile(userId: string): Promise<UserSingleResponse> {
    const user = getUserById(userId)
    if (!user) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'User not found' }
    }
    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      data: {
        id: user.id,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        phone: user.phone,
        avatar: user.avatar,
        role: user.role,
        emailVerified: user.emailVerified,
        createdAt: user.createdAt,
        updatedAt: user.updatedAt,
      },
    }
  }

  async update(userId: string, updates: Partial<UserResponse>): Promise<UserSingleResponse> {
    const user = getUserById(userId)
    if (!user) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'User not found' }
    }
    const updatedUser: UserResponse = {
      ...user,
      ...updates,
      updatedAt: new Date().toISOString(),
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: updatedUser }
  }

  async changePassword(_userId: string, _currentPassword: string, _newPassword: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number }> {
    return { isSuccess: true, isFailure: false, statusCode: 200 }
  }

  async requestPasswordReset(_email: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number }> {
    return { isSuccess: true, isFailure: false, statusCode: 200 }
  }
}

export const mockUserRepository = new MockUserRepository()