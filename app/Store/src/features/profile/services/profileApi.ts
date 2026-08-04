import { get, put } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'
import type { ProfileDetail, UpdateProfileRequest } from '../types/profile'

// GET api/store/profiles/profiles — the authenticated user's profile.
export function getProfile(): Promise<Result<ProfileDetail>> {
  return get<Result<ProfileDetail>>(ENDPOINTS.profiles)
}

// PUT api/store/profiles/profiles — update profile fields (upserts if absent).
export function updateProfile(req: UpdateProfileRequest): Promise<Result<ProfileDetail>> {
  return put<Result<ProfileDetail>>(ENDPOINTS.profiles, req)
}
