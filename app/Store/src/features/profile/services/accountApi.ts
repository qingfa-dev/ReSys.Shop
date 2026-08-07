import { del } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'

export function deleteProfile(): Promise<Result<void>> {
  return del<Result<void>>(ENDPOINTS.profiles)
}
