import { del } from '@/shared/api/client'
import type { Result } from '@/shared/types'

export class AccountApi {
  private static readonly BASE = '/api/storefront/customer/profiles'

  // Call: Send account deletion request to identity service
  static async deleteProfile(): Promise<Result<void>> {
    return await del<Result<void>>(`${this.BASE}`)
  }
}
