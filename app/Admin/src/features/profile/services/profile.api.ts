import apiClient from '@/shared/api/http/api.client'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { Profile, ProfileUpdateRequest } from '../types/profile.types'
import { PROFILES } from '@/shared/api/constants'

export const profileApi = {
  async get(): Promise<ApiResult<Profile>> {
    return apiClient.get(PROFILES)
  },
  async update(data: ProfileUpdateRequest): Promise<ApiResult<Profile>> {
    return apiClient.put(PROFILES, data)
  },
}
