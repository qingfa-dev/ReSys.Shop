import { del } from '@/shared/api/client'
import { PROFILES } from '@/shared/constants/api'
import type { Result } from '@/shared/types'

export class AccountApi {
  private static readonly BASE = `${PROFILES}/profiles`

  // Call: Send account deletion request to identity service
  static async deleteProfile(): Promise<Result<void>> {
    return await del<Result<void>>(`${this.BASE}`)
  }
}
