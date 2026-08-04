import { get, put } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'
import type { NotificationPreferences } from '../types/notification'

// GET api/store/profiles/notification-preferences.
export function getNotificationPreferences(): Promise<Result<NotificationPreferences>> {
  return get<Result<NotificationPreferences>>(ENDPOINTS.notificationPreferences)
}

// PUT api/store/profiles/notification-preferences — full three-boolean replacement.
export function updateNotificationPreferences(
  req: NotificationPreferences,
): Promise<Result<NotificationPreferences>> {
  return put<Result<NotificationPreferences>>(ENDPOINTS.notificationPreferences, req)
}
