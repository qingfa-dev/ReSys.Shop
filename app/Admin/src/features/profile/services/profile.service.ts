import apiClient from '@/shared/api/api.client'
import type { ApiResult } from '@/shared/api/api.types'
import type { Profile, ProfileUpdateRequest } from '../types/profile.types'

export const profileService = {
  async getProfile(): Promise<ApiResult<Profile>> {
    return apiClient.get('/api/admin/profile')
  },
  async updateProfile(data: ProfileUpdateRequest): Promise<ApiResult<Profile>> {
    return apiClient.put('/api/admin/profile', data)
  },
}
