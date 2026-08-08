import { get, post } from '@/shared/api/client'
import { IDENTITY } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type { SessionInfo } from '../types'

export class SessionApi {
  static async getSessions(): Promise<Result<SessionInfo[]>> {
    return await get<Result<SessionInfo[]>>(`${IDENTITY}/auth/sessions`)
  }

  static async revokeCurrentDevice(): Promise<Result<void>> {
    return await post<Result<void>>(`${IDENTITY}/auth/logout`, { revokeAll: false })
  }

  static async revokeAll(): Promise<Result<void>> {
    return await post<Result<void>>(`${IDENTITY}/auth/logout`, { revokeAll: true })
  }
}
