import { post } from '@/shared/api/client'
import type { Result } from '@/shared/types'

export class EmailApi {
  static async changeEmail(newEmail: string): Promise<Result<void>> {
    return await post('/api/storefront/identity/emails/change', { newEmail })
  }

  static async confirmEmail(token: string): Promise<Result<void>> {
    return await post('/api/storefront/identity/emails/confirm', { token })
  }

  static async resendVerification(): Promise<Result<void>> {
    return await post('/api/storefront/identity/emails/resend', {})
  }
}
