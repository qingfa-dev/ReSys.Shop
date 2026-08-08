import { post } from '@/shared/api/client'
import { IDENTITY } from '@/shared/constants/api'
import type { Result } from '@/shared/types'

export class EmailApi {
  static async changeEmail(newEmail: string): Promise<Result<void>> {
    return await post(`${IDENTITY}/emails/change`, { newEmail })
  }

  static async confirmEmail(token: string): Promise<Result<void>> {
    return await post(`${IDENTITY}/emails/confirm`, { token })
  }

  static async resendVerification(): Promise<Result<void>> {
    return await post(`${IDENTITY}/emails/resend`, {})
  }
}
