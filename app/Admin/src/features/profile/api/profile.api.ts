import apiClient from '@/shared/api/http/api.client'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { Profile } from '../types/profile.response.type'
import type { ProfileUpdateRequest } from '../types/profile.request.type'
import { PROFILES } from '@/shared/api/constants'
import { mapProfileResponse } from '../mappers/profile.mapper'

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
