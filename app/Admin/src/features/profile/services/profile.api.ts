import apiClient from '@/shared/api/http/api.client'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { Profile, ProfileUpdateRequest } from '../types/profile.types'
import { PROFILE } from '@/shared/api/constants'

export const profileApi = {
  async get(): Promise<ApiResult<Profile>> {
    return apiClient.get(PROFILE)
  },
  async update(data: ProfileUpdateRequest): Promise<ApiResult<Profile>> {
    return apiClient.put(PROFILE, data)
  },
}
