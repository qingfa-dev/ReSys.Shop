import type { Result } from '@/core/models/result'
import type { INotificationsRepository, NotificationPreference } from './notifications.repository.interface'

const defaultPreferences: NotificationPreference[] = [
  { id: 'pref-1', type: 'order_confirmation', email: true, sms: true, push: true, inApp: true },
  { id: 'pref-2', type: 'shipping_update', email: true, sms: true, push: true, inApp: true },
  { id: 'pref-3', type: 'promotions', email: false, sms: false, push: true, inApp: true },
  { id: 'pref-4', type: 'account_security', email: true, sms: true, push: true, inApp: true },
]

export class MockNotificationsRepository implements INotificationsRepository {
  private preferences = [...defaultPreferences]

  async getPreferences(): Promise<Result<NotificationPreference[]>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: [...this.preferences] }
  }

  async updatePreference(id: string, updates: Partial<NotificationPreference>): Promise<Result<NotificationPreference>> {
    const index = this.preferences.findIndex(p => p.id === id)
    if (index === -1) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Preference not found' }
    }
    this.preferences[index] = { ...this.preferences[index], ...updates } as NotificationPreference
    return { isSuccess: true, isFailure: false, statusCode: 200, data: this.preferences[index] }
  }

  async updateAllPreferences(preferences: NotificationPreference[]): Promise<Result<NotificationPreference[]>> {
    this.preferences = preferences.map(p => ({ ...p }))
    return { isSuccess: true, isFailure: false, statusCode: 200, data: [...this.preferences] }
  }
}

export const mockNotificationsRepository = new MockNotificationsRepository()
