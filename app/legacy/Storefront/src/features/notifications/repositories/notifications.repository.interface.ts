import type { Result } from '@/core/models/result'

export interface NotificationPreference {
  id: string
  type: string
  email: boolean
  sms: boolean
  push: boolean
  inApp: boolean
}

export interface INotificationsRepository {
  getPreferences(): Promise<Result<NotificationPreference[]>>
  updatePreference(id: string, updates: Partial<NotificationPreference>): Promise<Result<NotificationPreference>>
  updateAllPreferences(preferences: NotificationPreference[]): Promise<Result<NotificationPreference[]>>
}
