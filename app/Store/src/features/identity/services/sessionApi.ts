import { get, post } from '@/shared/api/client'
import type { Result } from '@/shared/types'
import type { SessionInfo } from '../types'

export class SessionApi {
  static async getSessions(): Promise<Result<SessionInfo[]>> {
    return await get<Result<SessionInfo[]>>('/api/storefront/identity/auth/sessions')
  }

  static async revokeCurrentDevice(): Promise<Result<void>> {
    return await post<Result<void>>('/api/storefront/identity/auth/logout', { revokeAll: false })
  }

  static async revokeAll(): Promise<Result<void>> {
    return await post<Result<void>>('/api/storefront/identity/auth/logout', { revokeAll: true })
  }
}
