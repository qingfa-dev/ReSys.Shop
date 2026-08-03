import { notificationsApiRepository } from '../repositories/notifications.api'
import type { INotificationsService } from './notifications.service.interface'
import type { NotificationPreference } from '../repositories/notifications.repository.interface'
import type { Result } from '@/core/models/result'

export class NotificationsService implements INotificationsService {
  private readonly repository = notificationsApiRepository

  async getPreferences(): Promise<Result<NotificationPreference[]>> {
    return this.repository.getPreferences()
  }

  async updatePreference(id: string, updates: Partial<NotificationPreference>): Promise<Result<NotificationPreference>> {
    return this.repository.updatePreference(id, updates)
  }

  async updateAllPreferences(preferences: NotificationPreference[]): Promise<Result<NotificationPreference[]>> {
    return this.repository.updateAllPreferences(preferences)
  }
}

export const notificationsService = new NotificationsService()
