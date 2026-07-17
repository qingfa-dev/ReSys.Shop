import apiClient from '@/shared/api/http/api.client'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { Profile } from '../types/profile.domain.types'
import type { ProfileUpdateRequest } from '../types/profile.request.types'
import { PROFILES } from '@/shared/api/constants'

export const profileRepository = {
  async get(): Promise<ServerResult<Profile>> {
    return apiClient.get(`${PROFILES}/profiles`).then(res => res.data as ServerResult<Profile>)
  },
  async update(data: ProfileUpdateRequest): Promise<ServerResult<Profile>> {
    return apiClient.put(`${PROFILES}/profiles`, data).then(res => res.data as ServerResult<Profile>)
  },
}
