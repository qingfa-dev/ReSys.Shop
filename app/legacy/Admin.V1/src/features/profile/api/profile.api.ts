import apiClient from '@/common/api/http/api.client'
import type { ServerResult } from '@/common/api/types/result.types'
import type { Profile } from '../types/profile.response'
import type { ProfileUpdateRequest } from '../types/profile.request'
import { PROFILES } from '@/common/api/constants'
import { mapProfileResponse } from '../models/profile.mapper'

export const profileRepository = {
  async get(): Promise<ServerResult<Profile>> {
    return apiClient.get(`${PROFILES}/profiles`).then(res => {
      const result = res.data as ServerResult<Profile>
      return { ...result, value: mapProfileResponse(result.value) }
    })
  },
  async update(data: ProfileUpdateRequest): Promise<ServerResult<Profile>> {
    return apiClient.put(`${PROFILES}/profiles`, data).then(res => {
      const result = res.data as ServerResult<Profile>
      return { ...result, value: result.value ? mapProfileResponse(result.value) : result.value }
    })
  },
}
