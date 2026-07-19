import type { ProfileParameters } from '../schemas/profile.schema'

export type ProfileUpdateRequest = ProfileParameters & {
  preferences?: ProfilePreferences
  notifications?: NotificationPreferences
}

import type { ProfilePreferences, NotificationPreferences } from './profile.response.type'
