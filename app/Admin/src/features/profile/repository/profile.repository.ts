import apiClient from '@/shared/api/http/api.client'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { Profile } from '../types/Profile.Response.Type'
import type { ProfileUpdateRequest } from '../types/Profile.Request.Type'
import { PROFILES } from '@/shared/api/constants'

export const profileRepository = {
  async get(): Promise<ServerResult<Profile>> {
    return apiClient.get(`${PROFILES}/profiles`).then(res => res.data as ServerResult<Profile>)
  },
  async update(data: ProfileUpdateRequest): Promise<ServerResult<Profile>> {
    return apiClient.put(`${PROFILES}/profiles`, data).then(res => res.data as ServerResult<Profile>)
  },
}
