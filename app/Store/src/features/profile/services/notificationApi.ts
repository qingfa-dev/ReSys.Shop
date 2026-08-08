import { get, put } from '@/shared/api/client'
import { PROFILES } from '@/shared/constants/api'
import { NotificationPreferencesSchema } from '../validations/notification'
import type { Result } from '@/shared/types'
import type { NotificationPreferences } from '../types'

export class NotificationApi {
  private static readonly BASE = `${PROFILES}/notifications`

  static async getNotificationPreferences(): Promise<Result<NotificationPreferences>> {
    const result = await get<Result<NotificationPreferences>>(`${this.BASE}/preferences`)
    if (!result.isSuccess) return result
    result.value = NotificationPreferencesSchema.parse(result.value)
    return result
  }

  static async updateNotificationPreferences(req: NotificationPreferences): Promise<Result<NotificationPreferences>> {
    const result = await put<Result<NotificationPreferences>>(`${this.BASE}/preferences`, req)
    if (!result.isSuccess) return result
    result.value = NotificationPreferencesSchema.parse(result.value)
    return result
  }
}
