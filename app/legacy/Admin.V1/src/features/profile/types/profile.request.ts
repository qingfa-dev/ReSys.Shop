import type { ProfileParameters } from '../types/profile.field'

export type ProfileUpdateRequest = ProfileParameters & {
  preferences?: ProfilePreferences
  notifications?: NotificationPreferences
}

import type { ProfilePreferences, NotificationPreferences } from './profile.response'
