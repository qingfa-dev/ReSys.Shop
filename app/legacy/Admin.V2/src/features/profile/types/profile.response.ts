export interface ProfileResponse {
  id: string
  userId: string
  firstName: string
  lastName: string
  email: string
  phone?: string | null
  avatarUrl?: string | null
  dateOfBirth?: string | null
  createdAt: string
  updatedAt: string
}
