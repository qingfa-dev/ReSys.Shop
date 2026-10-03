import type { Result } from '@/core/models/result'
import type { NotificationPreference } from '../repositories/notifications.repository.interface'

export interface INotificationsService {
  getPreferences(): Promise<Result<NotificationPreference[]>>
  updatePreference(id: string, updates: Partial<NotificationPreference>): Promise<Result<NotificationPreference>>
  updateAllPreferences(preferences: NotificationPreference[]): Promise<Result<NotificationPreference[]>>
}
