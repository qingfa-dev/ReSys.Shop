import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { INotificationsRepository, NotificationPreference } from './notifications.repository.interface'

export class NotificationsApiRepository extends BaseRepository implements INotificationsRepository {
  async getPreferences(): Promise<Result<NotificationPreference[]>> {
    return this.get<NotificationPreference[]>('/api/storefront/profiles/notification-preferences')
  }

  async updatePreference(id: string, updates: Partial<NotificationPreference>): Promise<Result<NotificationPreference>> {
    return this.patchPartial<NotificationPreference>('/api/storefront/profiles/notification-preferences', id, updates)
  }

  async updateAllPreferences(preferences: NotificationPreference[]): Promise<Result<NotificationPreference[]>> {
    return this.put<NotificationPreference[]>('/api/storefront/profiles/notification-preferences', preferences)
  }
}

export const notificationsApiRepository = new NotificationsApiRepository()
