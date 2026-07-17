import type { ProfilePreferences, NotificationPreferences } from './profile.domain.types'

export interface ProfileUpdateRequest {
  firstName?: string
  lastName?: string
  phoneNumber?: string
  dateOfBirth?: string
  gender?: string
  bio?: string
  preferences?: ProfilePreferences
  notifications?: NotificationPreferences
  acceptsEmailMarketing?: boolean
}
