// Context: Full profile entity returned by the profile API
export interface ProfileDetail {
  id: string
  userId: string
  fullName: string
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
  dateOfBirth: string | null
  preferences: Record<string, unknown> | null
  notifications: Record<string, boolean> | null
  emailConfirmed: boolean
  phoneNumberConfirmed: boolean
  createdAtUtc: string
  modifiedAtUtc: string | null
}

// Context: Request payload for profile update (PUT /profiles/profiles)
export interface UpdateProfileRequest {
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
}
