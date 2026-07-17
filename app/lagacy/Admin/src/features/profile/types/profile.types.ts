export interface Profile {
  id: string
  email: string
  firstName: string
  lastName: string
  phoneNumber?: string
  dateOfBirth?: string
  gender?: string
  bio?: string
  avatarUrl?: string
  preferences?: ProfilePreferences
  notifications?: NotificationPreferences
  isActive: boolean
  acceptsEmailMarketing?: boolean
  createdAtUtc: string
  modifiedAtUtc?: string
}

export interface ProfilePreferences {
  preferredStyle?: string
  preferredFit?: string
  favoriteColors?: string[]
  favoriteCategories?: string[]
  preferredBrands?: string[]
  sizeTop?: string
  sizeBottom?: string
  shoeSize?: string
}

export interface NotificationPreferences {
  enableSms?: boolean
  enableEmail?: boolean
  enableNewsfeeds?: boolean
}

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
