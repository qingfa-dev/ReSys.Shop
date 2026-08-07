import { post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'

export function changeEmail(newEmail: string): Promise<Result<void>> {
  return post<Result<void>>(ENDPOINTS.emailsChange, { newEmail })
}

export function confirmEmail(token: string): Promise<Result<void>> {
  return post<Result<void>>(ENDPOINTS.emailsConfirm, { token })
}

export function resendVerification(): Promise<Result<void>> {
  return post<Result<void>>(ENDPOINTS.emailsResend)
}
