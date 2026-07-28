import type { Result } from '@/core/models/result'

export interface ProfileResponse {
  id: string
  email: string
  first_name: string
  last_name: string
  display_name: string
  phone?: string
  avatar?: string
  date_of_birth?: string
  gender?: string
  created_at: string
  updated_at: string
}

export type ProfileSingleResponse = Result<ProfileResponse>
