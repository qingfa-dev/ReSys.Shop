import type { UserPreferences, AccountSettings, UserPreferencesSchemaType, AccountSettingsSchemaType } from '../types'

export function toUserPreferences(schema: UserPreferencesSchemaType): UserPreferences {
  return {
    currency: schema.currency,
    language: schema.language,
    timezone: schema.timezone,
    newsletter: schema.newsletter,
    notifications: schema.notifications ?? { email: true, sms: false, push: true },
  }
}

export function fromUserPreferences(prefs: UserPreferences): UserPreferencesSchemaType {
  return UserPreferencesSchema.parse(prefs)
}

export function toAccountSettings(schema: AccountSettingsSchemaType): AccountSettings {
  return {
    email: schema.email,
    firstName: schema.firstName,
    lastName: schema.lastName,
    phone: schema.phone,
    avatar: schema.avatar,
  }
}

export function fromAccountSettings(settings: AccountSettings): AccountSettingsSchemaType {
  return AccountSettingsSchema.parse(settings)
}

export function getFullName(settings: AccountSettings): string {
  return `${settings.firstName} ${settings.lastName}`
}

export function getInitials(settings: AccountSettings): string {
  const first = settings.firstName.charAt(0).toUpperCase()
  const last = settings.lastName.charAt(0).toUpperCase()
  return `${first}${last}`
}

import { UserPreferencesSchema, AccountSettingsSchema } from '../types/schemas'