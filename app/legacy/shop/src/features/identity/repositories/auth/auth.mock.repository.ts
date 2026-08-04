import type { AuthResponse, AuthTokensResponse } from '../../types/response'

export class MockAuthRepository {
  async login(credentials: { email: string; password: string; rememberMe?: boolean }): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number; data?: AuthResponse; message?: string }> {
    if (credentials.email && credentials.password) {
      const tokens: AuthTokensResponse = { accessToken: 'mock-access-token', refreshToken: 'mock-refresh-token', expiresIn: 3600 }
      return {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        data: {
          user: { id: 'user-1', email: credentials.email, firstName: 'John', lastName: 'Doe', role: 'customer', emailVerified: true, mfaEnabled: false, createdAt: '2026-01-01', updatedAt: '2026-01-01' },
          tokens,
        },
      }
    }
    return { isSuccess: false, isFailure: true, statusCode: 401, message: 'Invalid credentials' }
  }

  async register(info: { email: string; password: string; firstName: string; lastName: string; phone?: string }): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number; data?: AuthResponse; message?: string }> {
    if (info.email && info.password && info.firstName && info.lastName) {
      const tokens: AuthTokensResponse = { accessToken: 'mock-access-token-new', refreshToken: 'mock-refresh-token-new', expiresIn: 3600 }
      return {
        isSuccess: true,
        isFailure: false,
        statusCode: 201,
        data: {
          user: { id: `user-${Date.now()}`, email: info.email, firstName: info.firstName, lastName: info.lastName, role: 'customer', emailVerified: false, mfaEnabled: false, createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
          tokens,
        },
      }
    }
    return { isSuccess: false, isFailure: true, statusCode: 400, message: 'Invalid registration data' }
  }

  async logout(): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number }> {
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

export const mockAuthRepository = new MockAuthRepository()