export interface Profile {
  id: string
  email: string
  firstName: string
  lastName: string
  phone: string
}

export interface ProfileUpdateRequest {
  firstName: string
  lastName: string
  phone: string
}
