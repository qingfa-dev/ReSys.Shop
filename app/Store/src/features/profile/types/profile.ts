// Types mirror the storefront profile DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Profile.Features.Store.Profiles (service/Api):
// - GET/PUT api/store/profiles/profiles → ProfileDetailResponse
// Enums serialize as strings via JsonStringEnumConverter.

export interface ProfilePreferences {
  preferredStyle: string | null
  preferredFit: string | null
  favoriteColors: string[]
  favoriteCategories: string[]
  preferredBrands: string[]
  sizeTop: string | null
  sizeBottom: string | null
  shoeSize: string | null
}

// Flat three-boolean notification prefs (NOT a {channel, category, enabled} list).
export interface ProfileNotificationPreferences {
  enableSms: boolean
  enableEmail: boolean
  enableNewsfeeds: boolean
}

export interface ProfileDetail {
  id: string
  userId: string
  fullName: string
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
  dateOfBirth: string | null
  preferences: ProfilePreferences | null
  notifications: ProfileNotificationPreferences | null
  emailConfirmed: boolean
  phoneNumberConfirmed: boolean
  createdAtUtc: string
  modifiedAtUtc: string | null
  createdBy: string | null
  modifiedBy: string | null
}

// PUT body — ProfileRequest. NOTE: the update mapping writes Email unconditionally,
// so the client must always send the current email (prefilled from the profile).
export interface UpdateProfileRequest {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
}
