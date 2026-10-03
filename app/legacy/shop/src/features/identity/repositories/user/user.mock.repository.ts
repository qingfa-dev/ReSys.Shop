import type { UserResponse, UserSingleResponse } from '../../types/response'
import { getUserById } from '../../data/mock-users.data'

export class MockUserRepository {
  async getById(id: string): Promise<UserSingleResponse> {
    const user = getUserById(id)
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
        mfaEnabled: user.mfaEnabled,
        createdAt: user.createdAt,
        updatedAt: user.updatedAt,
      },
    }
  }

  async getProfile(userId: string): Promise<UserSingleResponse> {
    return this.getById(userId)
  }

  async updateProfile(userId: string, updates: Partial<UserResponse>): Promise<UserSingleResponse> {
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

  async update(userId: string, updates: Partial<UserResponse>): Promise<UserSingleResponse> {
    return this.updateProfile(userId, updates)
  }

  async changePassword(_userId: string, _currentPassword: string, _newPassword: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number }> {
    return { isSuccess: true, isFailure: false, statusCode: 200 }
  }

  async requestPasswordReset(_email: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number }> {
    return { isSuccess: true, isFailure: false, statusCode: 200 }
  }

  async enableMFA(_userId: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number; data?: { secret: string; qrCode: string } }> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: { secret: 'MOCK_SECRET_KEY', qrCode: 'mock-qr-code-data' } }
  }

  async verifyMFA(_userId: string, code: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number; message?: string }> {
    if (code === '123456') {
      return { isSuccess: true, isFailure: false, statusCode: 200 }
    }
    return { isSuccess: false, isFailure: true, statusCode: 400, message: 'Invalid MFA code' }
  }

  async disableMFA(_userId: string, code: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number; message?: string }> {
    if (code === '123456') {
      return { isSuccess: true, isFailure: false, statusCode: 200 }
    }
    return { isSuccess: false, isFailure: true, statusCode: 400, message: 'Invalid MFA code' }
  }
}

export const mockUserRepository = new MockUserRepository()