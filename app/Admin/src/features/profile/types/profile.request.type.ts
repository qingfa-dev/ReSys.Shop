import type { ProfileParameters } from '../schemas/Profile.Schema'

export type ProfileUpdateRequest = ProfileParameters & {
  preferences?: ProfilePreferences
  notifications?: NotificationPreferences
}

import type { ProfilePreferences, NotificationPreferences } from './Profile.Response.Type'
