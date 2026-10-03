import { get, patch } from '@/shared/api/client'
import { NotificationPreferencesSchema } from '../validations/notification'
import type { Result } from '@/shared/types'
import type { NotificationPreferences } from '../types'

export class NotificationApi {
  // Call: Fetch current notification channel preferences
  static async getNotificationPreferences(): Promise<Result<NotificationPreferences>> {
    const result = await get<Result<NotificationPreferences>>('/api/storefront/customer/notification-preferences')
    if (!result.isSuccess) return result
    // Transform: Validate response against notification schema
    result.value = NotificationPreferencesSchema.parse(result.value)
    return result
  }

  // Call: Persist updated notification channel preferences
  static async updateNotificationPreferences(req: NotificationPreferences): Promise<Result<NotificationPreferences>> {
    const result = await patch<Result<NotificationPreferences>>('/api/storefront/customer/notification-preferences', req)
    if (!result.isSuccess) return result
    result.value = NotificationPreferencesSchema.parse(result.value)
    return result
  }
}
