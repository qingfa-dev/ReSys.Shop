// Types mirror the storefront notification-preferences DTO exactly (camelCase JSON).
// Contract pinned from Module.Profile.Features.Store.NotificationPreferences:
// GET/PUT api/store/profiles/notification-preferences → ProfileNotificationPreferences.
// It is a FLAT three-boolean object — not a {channel, category, enabled} list.

export interface NotificationPreferences {
  enableSms: boolean
  enableEmail: boolean
  enableNewsfeeds: boolean
}

export const DEFAULT_NOTIFICATION_PREFERENCES: NotificationPreferences = {
  enableSms: true,
  enableEmail: true,
  enableNewsfeeds: true,
}
